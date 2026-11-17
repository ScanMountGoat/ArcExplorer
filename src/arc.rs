use std::{collections::HashMap, path::Path};

use rayon::prelude::*;
use smash_arc::{
    ArcFile, ArcLookup, FileDataIdx, FileNode, GLOBAL_LABELS, Hash40, Region, SearchCache,
};

pub const ROOT_PATH: &str = "/";

pub struct ArcData {
    pub arc: ArcFile,
    pub search_cache: SearchCache,
    pub file_path: String,
    pub shared_file_cache: HashMap<smash_arc::FileDataIdx, Vec<Hash40>>,
}

impl ArcData {
    pub fn from_path<P: AsRef<std::path::Path>>(path: P) -> Self {
        // TODO: avoid unwrap.
        let path = path.as_ref();
        let arc = ArcFile::open(path).unwrap();
        let start = std::time::Instant::now();
        let search_cache = arc.generate_search_cache();
        println!("Create search cache: {:?}", start.elapsed());

        let start = std::time::Instant::now();
        let shared_file_cache = generated_shared_file_cache(&arc);
        println!("Create shared file cache: {:?}", start.elapsed());

        Self {
            arc,
            search_cache,
            file_path: path.to_string_lossy().to_string(),
            shared_file_cache,
        }
    }
}

#[derive(Debug, Clone)]
pub enum FileNodeInfo {
    Folder {
        hash: Hash40,
        name: String,
        path: String,
    },
    File {
        hash: Hash40,
        name: String,
        path: String,
        offset: u64,
        description: String,
        is_compressed: bool,
        is_regional: bool,
        is_shared: bool,
        // TODO: usize for bytes and add formatting logic with tests
        compressed_size: String,
        decompressed_size: String,
    },
}

impl FileNodeInfo {
    pub fn name(&self) -> &str {
        match self {
            FileNodeInfo::Folder { name, .. } => name,
            FileNodeInfo::File { name, .. } => name,
        }
    }

    pub fn path(&self) -> &str {
        match self {
            FileNodeInfo::Folder { path, .. } => path,
            FileNodeInfo::File { path, .. } => path,
        }
    }

    pub fn description(&self) -> &str {
        match self {
            FileNodeInfo::Folder { .. } => "",
            FileNodeInfo::File { description, .. } => description,
        }
    }

    pub fn compressed_size(&self) -> &str {
        match self {
            FileNodeInfo::Folder { .. } => "",
            FileNodeInfo::File {
                compressed_size, ..
            } => compressed_size,
        }
    }

    pub fn decompressed_size(&self) -> &str {
        match self {
            FileNodeInfo::Folder { .. } => "",
            FileNodeInfo::File {
                decompressed_size, ..
            } => decompressed_size,
        }
    }

    pub fn is_regional(&self) -> bool {
        match self {
            FileNodeInfo::Folder { .. } => false,
            FileNodeInfo::File { is_regional, .. } => *is_regional,
        }
    }

    pub fn is_shared(&self) -> bool {
        match self {
            FileNodeInfo::Folder { .. } => false,
            FileNodeInfo::File { is_shared, .. } => *is_shared,
        }
    }

    pub fn shared_files(&self, arc: &ArcData) -> Vec<String> {
        match self {
            FileNodeInfo::Folder { .. } => Vec::new(),
            FileNodeInfo::File { hash, .. } => {
                if let Some(file_data_index) = file_data_index(&arc.arc, *hash) {
                    // TODO: avoid clone
                    let hashes = arc.shared_file_cache[&file_data_index].clone();
                    files_from_hashes(&arc.arc, hashes)
                        .into_iter()
                        .map(|f| f.path().to_string())
                        .collect()
                } else {
                    Vec::new()
                }
            }
        }
    }
}

fn file_data_index(arc: &ArcFile, hash: Hash40) -> Option<FileDataIdx> {
    if let Ok(file_info) = arc.get_file_info_from_hash(hash) {
        let file_in_folder = arc.get_file_in_folder(file_info, smash_arc::Region::UsEnglish);
        Some(file_in_folder.file_data_index)
    } else {
        None
    }
}

fn generated_shared_file_cache(arc: &ArcFile) -> HashMap<smash_arc::FileDataIdx, Vec<Hash40>> {
    // Use a parallel map-reduce to quickly group hashes with the same file data index.
    // This speeds up finding the shared files later compared to doing a lookup for each file.
    // https://github.com/jam1garner/smash-arc/blob/23418af7e58dba2c0bb2aafc1341e230643c3da9/src/lookups.rs#L206-L233
    arc.get_file_hash_to_path_index()
        .par_iter()
        .fold(
            || HashMap::new(),
            |mut map: HashMap<_, Vec<_>>, hash_to_path| {
                let hash = hash_to_path.hash40();
                if let Some(file_data_index) = file_data_index(arc, hash) {
                    map.entry(file_data_index).or_default().push(hash);
                    map
                } else {
                    map
                }
            },
        )
        .reduce(
            || HashMap::new(),
            |mut map1, map2| {
                for (key, values) in map2 {
                    map1.entry(key).or_default().extend_from_slice(&values);
                }
                map1
            },
        )
}

pub fn load_files(
    arc: &ArcFile,
    hash: impl Into<Hash40>,
    merge_trailing_slash: bool,
) -> Option<Vec<FileNodeInfo>> {
    if merge_trailing_slash {
        // TODO: load with and without trailing slash and merge?
        // TODO: merge folders with names that differ by a trailing slash?
    }
    let dir = arc.get_dir_listing(hash)?;

    // TODO: merge by trailing slash if enabled in preferences
    let files = load_file_nodes(arc, dir);
    Some(files)
}

fn load_file_nodes(arc: &ArcFile, dir: &[FileNode]) -> Vec<FileNodeInfo> {
    dir.iter()
        .map(|f| match f {
            FileNode::Dir(hash) => folder_info(*hash),
            FileNode::File(hash) => file_info(arc, *hash),
        })
        .collect()
}

fn file_info(arc: &ArcFile, hash: Hash40) -> FileNodeInfo {
    // TODO: use the current region.
    if let Ok(info) = arc.get_file_metadata(hash, Region::UsEnglish) {
        let (path, name, extension) = if info.is_stream {
            let stream_file_path = hash_name(info.path_hash);
            let stream_file_name = Path::new(&stream_file_path)
                .file_name()
                .unwrap_or_default()
                .to_string_lossy()
                .to_string();
            // ARC extensions don't contain the '.' at the beginning.
            let stream_file_extension = Path::new(&stream_file_path)
                .extension()
                .unwrap_or_default()
                .to_string_lossy()
                .to_string();
            (stream_file_path, stream_file_name, stream_file_extension)
        } else {
            // Recreate the absolute hash from the filename and parent directory.
            // This allows for using the smaller hash file.
            let parent = hash_name(info.parent_hash);
            let name = hash_name(info.file_name_hash);
            let extension = hash_name(info.ext_hash);

            // Always combine the parent and file name to get the full path.
            // Both hashes present: "a/b/c/d.ext"
            // Parent missing: "0x..../d.ext"
            // File missing: "a/b/c/0x..."
            let path = Path::new(&parent).join(&name).to_string_lossy().to_string();
            (path, name, extension)
        };

        FileNodeInfo::File {
            hash,
            name,
            path,
            offset: info.offset,
            description: ext_description(&extension).to_string(),
            is_compressed: info.is_compressed,
            is_regional: info.is_regional,
            is_shared: info.is_shared,
            compressed_size: info.comp_size.to_string(),
            decompressed_size: info.decomp_size.to_string(),
        }
    } else {
        // TODO: How to handle missing data.
        FileNodeInfo::File {
            hash,
            name: hash_name(hash),
            path: hash_name(hash),
            offset: 0,
            description: "File".to_string(),
            is_compressed: false,
            is_regional: false,
            is_shared: false,
            compressed_size: String::new(),
            decompressed_size: String::new(),
        }
    }
}

fn folder_info(hash: Hash40) -> FileNodeInfo {
    let path = hash_name(hash);
    let name = Path::new(&path)
        .file_name()
        .unwrap_or_default()
        .to_string_lossy()
        .to_string();
    FileNodeInfo::Folder { hash, name, path }
}

fn ext_description(ext: &str) -> &str {
    match ext {
        "arc" => "SARC Archive",
        "bfotf" => "OpenType Font",
        "bfttf" => "TrueType Font",
        "bin" => "Binary Data",
        "bntx" => "Texture",
        "csb" => "Common Sound Table",
        "eff" => "Effects Container",
        "fnv" => "Fighter Number Volume",
        "h264" => "MPEG-4 Video",
        "lc" => "Compiled Lua Script",
        "lvd" => "Stage Level Data",
        "msbt" => "Message Text",
        "nro" => "Executable/lua2cpp Moveset",
        "nuanmb" => "Animation Data",
        "nufxlb" => "Shader Data",
        "nuhlpb" => "Helper Bone Data",
        "numatb" => "Material Container",
        "numdlb" => "Model Data",
        "numshb" => "Mesh Data",
        "numshexb" => "Mesh Extension Data",
        "nus3audio" => "Sound Banks",
        "nus3bank" => "Sound Banks",
        "nus3conf" => "nus3conf",
        "nushdb" => "Compiled Shader",
        "nusktb" => "Model Skeleton",
        "nusrcmdlb" => "nusrcmdlb",
        "nurpdb" => "Render Pass Data",
        "nutexb" => "Texture",
        "prc" => "Parameters",
        "shpc" => "shpc",
        "shpcanim" => "shpcanim",
        "sli" => "Sound Label Info",
        "spt" => "Sound Priority Table",
        "sqb" => "Sequence Bank",
        "stdat" => "Stage Data",
        "stprm" => "Stage Parameters",
        "svt" => "Sound Volume Table",
        "tonelabel" => "Sound Entry Lookup Table",
        "webm" => "Video",
        "xmb" => "Rendering Data",
        _ => "",
    }
}

pub fn search_arc(
    arc: &ArcFile,
    search_cache: &SearchCache,
    search_term: &str,
) -> Vec<FileNodeInfo> {
    let matches = search_cache.search(search_term, &GLOBAL_LABELS.read(), 2000);

    files_from_hashes(arc, matches)
}

fn files_from_hashes(arc: &ArcFile, hashes: Vec<Hash40>) -> Vec<FileNodeInfo> {
    hashes
        .into_iter()
        .map(|hash| {
            // TODO: Is there a better way to detect files vs folders?
            if arc.get_dir_info_from_hash(hash).is_ok() {
                folder_info(hash)
            } else {
                file_info(arc, hash)
            }
        })
        .collect()
}

pub fn arc_version(arc: &ArcFile) -> u32 {
    (arc.file_system.fs_header.version & 0xF0000) >> 16
}

pub fn arc_file_count(arc: &ArcFile) -> usize {
    arc.file_system.file_paths.len()
}

fn hash_name(hash: Hash40) -> String {
    hash.global_label()
        .unwrap_or_else(|| format!("{:010X}", hash.0))
}

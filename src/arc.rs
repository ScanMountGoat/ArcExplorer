use std::path::Path;

use smash_arc::{ArcFile, ArcLookup, FileNode, Hash40, Region};

pub const ROOT_PATH: &str = "/";

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
    let files = dir
        .iter()
        .map(|f| match f {
            FileNode::Dir(hash) => {
                let path = hash_name(*hash);
                let name = Path::new(&path)
                    .file_name()
                    .unwrap_or_default()
                    .to_string_lossy()
                    .to_string();
                FileNodeInfo::Folder {
                    hash: *hash,
                    name,
                    path,
                }
            }
            FileNode::File(hash) => {
                // TODO: use the current region.
                if let Ok(info) = arc.get_file_metadata(*hash, Region::UsEnglish) {
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

                    // TODO: description.
                    FileNodeInfo::File {
                        hash: *hash,
                        name,
                        path,
                        offset: info.offset,
                        description: "File".to_string(),
                        is_compressed: info.is_compressed,
                        compressed_size: info.comp_size.to_string(),
                        decompressed_size: info.decomp_size.to_string(),
                    }
                } else {
                    // TODO: How to handle missing data.
                    FileNodeInfo::File {
                        hash: *hash,
                        name: hash_name(*hash),
                        path: hash_name(*hash),
                        offset: 0,
                        description: "File".to_string(),
                        is_compressed: false,
                        compressed_size: String::new(),
                        decompressed_size: String::new(),
                    }
                }
            }
        })
        .collect();
    files
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

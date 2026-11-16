use serde::{Deserialize, Serialize};

// Match the original C# JSON format for compatibility.
#[derive(Debug, Serialize, Deserialize)]
#[serde(default, rename_all = "PascalCase")]
pub struct ApplicationPreferences {
    pub start_maximized: bool,
    pub merge_trailing_slash: bool,
    pub theme: String,
    pub display_format: String, // TODO: parse this
    pub current_hashes_commit_sha: String,
    pub extract_location: String,
    pub arc_startup_location: Option<String>,
    pub arc_region: String,                    // TODO: parse this
    pub last_hashes_update_check_time: String, // TODO: how to parse this?
    pub arc_ip_address: Option<String>,
}

impl Default for ApplicationPreferences {
    fn default() -> Self {
        Self {
            start_maximized: false,
            merge_trailing_slash: true,
            theme: Default::default(),                     // TODO: dark
            display_format: Default::default(),            // TODO: decimal
            current_hashes_commit_sha: Default::default(), // TODO: set hashes commit for preloaded hashes
            extract_location: "export".to_string(),
            arc_startup_location: None,
            arc_region: Default::default(), // TODO: US English
            last_hashes_update_check_time: Default::default(), // TODO: datetime that is always before current time
            arc_ip_address: None,
        }
    }
}

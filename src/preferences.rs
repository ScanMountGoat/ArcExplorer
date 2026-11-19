use serde::{Deserialize, Deserializer, Serialize, Serializer};
use smash_arc::Region;

// Match the original C# JSON format for compatibility.
#[derive(Debug, Serialize, Deserialize)]
#[serde(default, rename_all = "PascalCase")]
pub struct ApplicationPreferences {
    pub start_maximized: bool,
    pub merge_trailing_slash: bool,
    pub theme: VisualTheme,
    pub display_format: IntegerDisplayFormat,
    pub current_hashes_commit_sha: String,
    pub extract_location: String,
    pub arc_startup_location: Option<String>,
    #[serde(
        serialize_with = "serialize_region",
        deserialize_with = "deserialize_region"
    )]
    pub arc_region: Region,
    pub last_hashes_update_check_time: String, // TODO: how to parse this?
    pub arc_ip_address: Option<String>,
}

impl Default for ApplicationPreferences {
    fn default() -> Self {
        Self {
            start_maximized: false,
            merge_trailing_slash: true,
            theme: VisualTheme::Dark,
            display_format: IntegerDisplayFormat::Decimal,
            current_hashes_commit_sha: Default::default(), // TODO: set hashes commit for preloaded hashes
            extract_location: "export".to_string(),
            arc_startup_location: None,
            arc_region: Region::UsEnglish,
            last_hashes_update_check_time: Default::default(), // TODO: datetime that is always before current time
            arc_ip_address: None,
        }
    }
}

#[derive(Debug, Serialize, Deserialize)]
pub enum VisualTheme {
    Light,
    Dark,
}

#[derive(Debug, Serialize, Deserialize)]
pub enum IntegerDisplayFormat {
    Decimal,
    Hexadecimal,
    Binary,
}

// The original C# code uses a simple string enum converter.
fn serialize_region<S>(region: &Region, serializer: S) -> Result<S::Ok, S::Error>
where
    S: Serializer,
{
    match region {
        Region::None => serializer.serialize_str("None"),
        Region::Japanese => serializer.serialize_str("Japanese"),
        Region::UsEnglish => serializer.serialize_str("UsEnglish"),
        Region::UsFrench => serializer.serialize_str("UsFrench"),
        Region::UsSpanish => serializer.serialize_str("UsSpanish"),
        Region::EuEnglish => serializer.serialize_str("EuEnglish"),
        Region::EuFrench => serializer.serialize_str("EuFrench"),
        Region::EuSpanish => serializer.serialize_str("EuSpanish"),
        Region::EuGerman => serializer.serialize_str("EuGerman"),
        Region::EuDutch => serializer.serialize_str("EuDutch"),
        Region::EuItalian => serializer.serialize_str("EuItalian"),
        Region::EuRussian => serializer.serialize_str("EuRussian"),
        Region::Korean => serializer.serialize_str("Korean"),
        Region::ChinaChinese => serializer.serialize_str("ChinaChinese"),
        Region::TaiwanChinese => serializer.serialize_str("TaiwanChinese"),
    }
}

fn deserialize_region<'de, D>(deserializer: D) -> Result<Region, D::Error>
where
    D: Deserializer<'de>,
{
    let s = String::deserialize(deserializer)?;
    match s.as_str() {
        "None" => Ok(Region::None),
        "Japanese" => Ok(Region::Japanese),
        "UsEnglish" => Ok(Region::UsEnglish),
        "UsFrench" => Ok(Region::UsFrench),
        "UsSpanish" => Ok(Region::UsSpanish),
        "EuEnglish" => Ok(Region::EuEnglish),
        "EuFrench" => Ok(Region::EuFrench),
        "EuSpanish" => Ok(Region::EuSpanish),
        "EuGerman" => Ok(Region::EuGerman),
        "EuDutch" => Ok(Region::EuDutch),
        "EuItalian" => Ok(Region::EuItalian),
        "EuRussian" => Ok(Region::EuRussian),
        "Korean" => Ok(Region::Korean),
        "ChinaChinese" => Ok(Region::ChinaChinese),
        "TaiwanChinese" => Ok(Region::TaiwanChinese),
        _ => Err(serde::de::Error::custom("invalid region")),
    }
}

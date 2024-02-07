extern crate cbindgen;

use std::{env, path::Path};

fn main() {
    let crate_dir = env::var("CARGO_MANIFEST_DIR").unwrap();
    let root = Path::new(crate_dir.as_str());
    let mut config = cbindgen::Config::from_file(root.join("cbindgen.toml").as_path()).unwrap();
    config.cpp_compat = true;
    let changed = cbindgen::Builder::new()
        .with_config(config)
        .with_crate(crate_dir)
        .with_pragma_once(true)
        .with_language(cbindgen::Language::C)
        .generate()
        .expect("Unable to generate bindings")
        .write_to_file("include/delta-lake-bridge.h");

    // If this changed and an env var disallows change, error
    if let Ok(env_val) = env::var("DELTA_LAKE_BRIDGE_DISABLE_HEADER_CHANGE") {
        if changed && env_val == "true" {
            println!("cargo:warning=bridge's header file changed unexpectedly from what's on disk");
            std::process::exit(1);
        }
    }
}

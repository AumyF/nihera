{
  description = "Flake utils demo";

  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system}; in
      rec {
        packages = flake-utils.lib.flattenTree { };
        devShells.default = pkgs.mkShell {
          buildInputs = [ pkgs.dotnet-sdk_6 pkgs.ffmpeg pkgs.pkg-config pkgs.libopus pkgs.libsodium];
          shellHook = ''
            export LD_LIBRARY_PATH=${pkgs.libopus}/lib:${pkgs.libsodium}/lib
          '';
        };
      }
    );
}

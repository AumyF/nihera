{
  description = "Flake utils demo";

  inputs.flake-utils.url = "github:numtide/flake-utils";

  outputs = { self, nixpkgs, flake-utils }:
    flake-utils.lib.eachDefaultSystem (system:
      let pkgs = nixpkgs.legacyPackages.${system}; in
      rec {
        packages = flake-utils.lib.flattenTree rec {
          nihera = pkgs.buildDotnetModule rec {
            pname = "nihera-chang";
            version = "0.0.1";

            src = ./.;

            projectFile = "NiheraChang/NiheraChang.fsproj";
            nugetDeps = ./nix/deps.nix;

            dotnet-sdk = pkgs.dotnetCorePackages.sdk_6_0;
            dotnet-runtime = pkgs.dotnetCorePackages.runtime_6_0;

            runtimeDeps = with pkgs; [ ffmpeg libopus libsodium ];
          };
        };
        devShells.default = pkgs.mkShell {
          buildInputs = [
            pkgs.dotnet-sdk_6
            pkgs.nuget-to-nix

            pkgs.ffmpeg
            pkgs.libopus
            pkgs.libsodium
          ];
          shellHook = ''
            export LD_LIBRARY_PATH=${pkgs.libopus}/lib:${pkgs.libsodium}/lib
          '';
        };
      }
    );
}

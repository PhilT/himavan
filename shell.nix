with import <nixpkgs> {};

mkShell {
  name = "dotnet";
  packages = [
    dotnet-sdk_7
    ncurses
  ];
  buildInputs = with pkgs; [
    ncurses
  ];

  # Workaround as Silk.NET and FreeTypeSharp need repackaging for Nix
  LD_LIBRARY_PATH="${ncurses}/lib";
}

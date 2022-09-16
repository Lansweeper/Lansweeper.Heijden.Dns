const { version: previousVersion } = require("./package.json");
const projectfolder = "src/";
const csprojfile = "Lansweeper.Heijden.Dns/Lansweeper.Heijden.Dns.csproj";
const nugetpackage = "Lansweeper.Heijden.Dns";
const gitprimarybranch = "master";

module.exports = {
  "branches": [
    gitprimarybranch
  ],
  "plugins": [
    "@semantic-release/commit-analyzer",
    "@semantic-release/release-notes-generator",
    [
      "@semantic-release/changelog",
      {
        "changelogFile": "CHANGELOG.md"
      }
    ],
    "@semantic-release/npm",
    [
      "@semantic-release/exec",
      {
        "prepareCmd": `sed -i -E 's/<Version>${previousVersion}/<Version>\${nextRelease.version}/' ${projectfolder}${csprojfile} && \
          git add ${projectfolder}${csprojfile} && \
          git commit -m ":bookmark: Bump from ${previousVersion} to \${nextRelease.version} in ${projectfolder}${csprojfile}" && \
          dotnet pack ${projectfolder}${csprojfile} --configuration Release && \
          dotnet nuget push ./${projectfolder}bin/Release/netstandard2.0/${nugetpackage}.\${nextRelease.version}.nupkg --source \"github\" --api-key $GITHUB_TOKEN`
      }
    ],
    [
      "@semantic-release/git",
      {
        "assets": [
          "CHANGELOG.md",
          "package.json"
        ],
        "message": ":bookmark: Release ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}"
      }
    ],
    "@semantic-release/github"
  ]
}

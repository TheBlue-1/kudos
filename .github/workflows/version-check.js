const ChildProcess = require("child_process");
const core = require('@actions/core')
main();
function main(){
if(!isVersionChanged())
core.setFailed(`Version must be changed!`)}

function isVersionChanged(){
    var changes=ChildProcess.execSync("git diff HEAD master -- Kudos/Kudos.csproj")
    console.log(changes);
    return true;
}
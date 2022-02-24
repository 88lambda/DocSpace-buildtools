/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


const nconf = require("nconf"),
    path = require("path"),
    fs = require("fs");

nconf.argv()
    .env()
    .file("config",path.join(__dirname, "config.json"));

if (nconf.get("NODE_ENV") !== "development" && fs.existsSync(path.join(__dirname, nconf.get("NODE_ENV") + ".json"))) {
    nconf.file("config", path.join(__dirname, nconf.get("NODE_ENV") + ".json"));
}

getAndSaveAppsettings();

module.exports = nconf;


function getAndSaveAppsettings(){
    var appsettings = nconf.get("app").appsettings;
    var env = nconf.get("app").environment;
    var valueEnv = nconf.get(env);
    var fileWithEnv = path.join(__dirname, appsettings, 'appsettings.' + valueEnv + '.json');

    if(fs.existsSync(fileWithEnv)){
        nconf.file("appsettings", fileWithEnv);
    }
    else{
        nconf.file("appsettings", path.join(__dirname, appsettings, 'appsettings.json'));
    }
    nconf.file("appsettingsServices", path.join(__dirname, appsettings, 'appsettings.services.json'));
}
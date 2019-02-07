const Discord = require("discord.js");
const testBot = new Discord.Client();

const fs = require('fs');
const fs2 = require('fs');
const token = fs2.readFileSync("Assets/Bot/token.txt").toString();
var article;
var article_alpha = "a";

setInterval(() => {
    article = fs.readFileSync("Assets/Bot/bot.txt");
    var lineArray = article.toString().split('\n');
    var theChannel = testBot.channels.get('466450753045921792');
    if (theChannel != null && article_alpha != article.toString() && lineArray[1] != undefined) {
        console.log("success");
        theChannel.send("ᅟᅠᅟᅠᅟᅠ" + lineArray[0] + "\n          " + lineArray[1] + "\n       " + lineArray[2] + "\n   " + lineArray[3] + "\n" + lineArray[4] + "\n   " + lineArray[5] + "\n       " + lineArray[6] + "\n          " + lineArray[7] + "\n             " + lineArray[8] + "\nᅟᅠ");
    }
    if (theChannel != null) {
        article_alpha = article.toString();
    }
}, 1000);

testBot.login(token);
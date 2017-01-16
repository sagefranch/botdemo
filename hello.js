var builder = require('botbuilder');

//create connector
var connector = new builder.ConsoleConnector().listen();

//create bot 
var bot = new builder.UniversalBot(connector);

//add dialog
/*bot.dialog('/', function(session){
    session.send('Hello bot!');
});*/

/*bot.dialog('/', [
    function(session){
        builder.Prompts.text(session, "Hello, what is your name?");
    },
    function(session, results){
        session.send("Hello, " + results.response);
    }
]);*/

bot.dialog('/', [
    function(session, args, next){
        if(!session.userData.name){
            session.beginDialog('/profile');
        }else{
            next();
        }
    },
    function(session, results){
        session.send("Hello, " + session.userData.name);
    }
]);

bot.dialog('/profile', [
    function(session){
        builder.Prompts.text(session, "What is your name? ");
    },
    function(session, results){
        session.userData.name = results.response;
        session.endDialog();
    }
]);
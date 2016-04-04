# Sentro
An automated betting bot to place bets on saltybet.com, started by Verslapper on 31 March 2016<br/>
<img src='http://vignette1.wikia.nocookie.net/mugen/images/5/53/Sentroidle.gif/revision/latest?cb=20140422113520' />

# Usage
Using a request-sniffing tool such as Fiddler, or by examining the requests in a web development tool such as Firebug, determine the cookies sent with your betting requests to saltybet.com.<br/>
These will include a _cfduid, a PHPSESSID, and possible a _ga (if Illuminati). These will identify your account for betting.<br/>
Include this string as an argument to the Sentro exe file.<br/>
Run Sentro from the cmd prompt (e.g. c:\sentro\sentro\bin\release>sentro __cfduid=dd3567adc0bc7998fde01064741f2789a1456980891; _ga=GA1.2.309934278.1456987010; PHPSESSID=s73psg9qotrds7elqbisto0446)

# Operation
Sentro will place bets on your behalf, using available winrate information (if relevant), contextual information about your balance (to help escape the mines) and the betting mode (going all-in on tourneys more frequently) and output the analysis and actions to the console.

# TSR_Loader

## Description :  
This Launcher will start `Transformers2.exe` binary and modify it's memory so that it can run standalone without the original `Shell.exe` application.
The genuine `SERVICE` menu beeing hosted by the `Shell.exe`, it won't be accessible anymore, so a configurator tool to replace those settings is included in the loader package.  
<br> 
⚠️   
<b>The loader is just booting the game, and does not handle inputs and controls.  
[DemulShooter](https://github.com/argonlefou/DemulShooter/wiki/Ringedge-2#transformers--shadow-rising) is needed for that.
</b>   
⚠️
<br><br>

## Installation instructions :
1. Unzip `Transformers2_Launcher.exe` and `Transformers2_Configurator.exe` in the main game folder, where `Transformers2.exe` game file is located.
  
2. Run `Transformers2_Configurator.exe` and save changes you made.
  
3. Run `Transformers2_Launcher.exe` and enjoy the game!  
<br>

## F.A.Q:
Q : Where can I find detailled explanations of the available options ?  
A : Look for the arcade cabinet user manual on Google, you'll easilly find them.

Q : I have issue starting the game  
A : The game may need you to install [DirectX End-User Runtimes (June 2010)](https://www.microsoft.com/en-us/download/details.aspx?id=8109) to have needed graphics/sound dependencies.  
<br>You can also run `Transformers2_Launcher.exe -v` to generate a debug file that may help me find if something is going wrong on the Launcher side.
<br><br>

## Configuration Items
<table>
  <tr><td align="center" colspan="2"><b>COINS ASSIGNMENTS</b></td></tr>
  <tr>
    <td>Freeplay</td>
    <td>Obvious</td>
  </tr>
  <tr>
    <td>Entry type</td>
    <td>Select the type of payment interface </td>
  </tr>
</table>

<table>
  <tr><td align="center" colspan="2"><b>GAME ASSIGNMENTS</b></td></tr>
  <tr>
    <td>Language</td>
    <td>Graphics language in game</td>
  </tr>
  <tr>
    <td>Game Difficulty</td>
    <td>Obvious</td>
  </tr>
  <tr>
    <td>Advertise Sound</td>
    <td>Sound level during attract mode</td>
  </tr>
  
  <tr>
    <td>Revival</td>
    <td>If "ON", both player can't time simultaneously</td>
  </tr>
  <tr>
    <td>Player1 Gun Reaction</td>
    <td>Enable/Disable recoil output for P1 controller</td>
  </tr>
  <tr>
    <td>Player2 Gun Reaction</td>
    <td>Enable/Disable recoil output for P2 controller</td>
  </tr>
  <tr>
    <td>Continue Countdown</td>
    <td>Countdown time in seconds for the Continue screen</td>
  </tr>
  <tr>
    <td>Ennemy Boost</td>
    <td>Obvious</td>
  </tr>
  <tr>
    <td>1ST Minute Gameplay	</td>
    <td>Invincibility delay when a player enters a game</td>
  </tr>
  <tr>
    <td>Kids Mode</td>
    <td>Remove P1 and P2 guns from screen</td>
  </tr>
  <tr>
    <td>Stage Select</td>
    <td>Enable/Disable "Stage Selection Screen" at the start of Level 2</td>
  </tr>

  <tr>
    <td>English Subtitles</td>
    <td>Obvious</td>
  </tr>
  <tr>
    <td>Swipe Card To play	</td>
    <td>Replace "INSERT COIN" by "SWIPE CARD" if credits are needed</td>
  </tr>
</table>

## Misc. notes :

Default game resolution is 1920x1080.  
Changing it may result in some graphic bug where the ennemy targets to shoot at are displayed on screen at the wrong location.
<br>
If possible, it's best to keep original resolution and upscale the content thanks to dedicated apps liike DgVoodoo or ReShade




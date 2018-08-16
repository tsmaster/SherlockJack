using System.Collections.Generic;
using UnityEngine;

public class StartGameMode : GameModeBase
{
    public StartGameMode ()
    {
        Globals.TheMission = new Mission();
        Globals.TheMission.stagesComplete = 0;
        Globals.TheCity = new City();

        Globals.TheCity.Start(3);
    }

    override public void Update(float seconds)
    {
        base.Update(seconds);
    }

    override public void Draw(Color[] outbuf)
    {
    }

    override public bool IsComplete()
    {
        return true;
    }

    override public GameModeBase GetNextMode()
    {
        List<LineDesc> lines = CharacterTextMode.MakeIntroDialog();
        return new CharacterTextMode(Globals.FontTex, lines);
    }
}

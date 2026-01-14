using Comfort.Common;
using EFT;
using EFT.Console.Core;

namespace ContinuousHealing;

public abstract class DebugCommands
{
    [ConsoleCommand("damageLimbs")]
    public static void DamageLimbs()
    {
        if (Singleton<GameWorld>.Instantiated)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld.MainPlayer != null)
            {
                gameWorld.MainPlayer.ActiveHealthController.ApplyDamage(EBodyPart.LeftArm, 20, default);
                gameWorld.MainPlayer.ActiveHealthController.ApplyDamage(EBodyPart.RightArm, 20, default);
            }
        }
    }

    [ConsoleCommand("restoreLimbs")]
    public static void RestoreLimbs()
    {
        if (Singleton<GameWorld>.Instantiated)
        {
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld.MainPlayer != null)
            {
                gameWorld.MainPlayer.ActiveHealthController.RestoreFullHealth();
            }
        }
    }
}

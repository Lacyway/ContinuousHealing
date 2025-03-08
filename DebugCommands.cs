using Comfort.Common;
using EFT;
using EFT.Console.Core;

namespace ContinuousHealing
{
    public class DebugCommands
    {
        [ConsoleCommand("damageLimbs")]
        public static void DamageLimbs()
        {
            if (Singleton<GameWorld>.Instantiated)
            {
                GameWorld gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld.MainPlayer != null)
                {
                    gameWorld.MainPlayer.ActiveHealthController.ApplyDamage(EBodyPart.LeftArm, 20, default);
                    gameWorld.MainPlayer.ActiveHealthController.ApplyDamage(EBodyPart.RightArm, 20, default);
                }
            }
        }
    }
}

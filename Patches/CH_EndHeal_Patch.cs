using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace ContinuousHealing.Patches
{
    internal class CH_EndHeal_Patch : ModulePatch
    {
        private static FieldInfo playerField;

        public static int Animation = 0;
        public static bool CancelRequested = false;

        protected override MethodBase GetTargetMethod()
        {
            playerField = AccessTools.Field(typeof(Player.MedsController), "_player");
            return typeof(Player.MedsController.Class1172).GetMethod(nameof(Player.MedsController.Class1172.method_8));
        }

        [PatchPrefix]
        public static bool Prefix(Player.MedsController.Class1172 __instance, Player.MedsController ___medsController_0, IEffect effect, Callback<IOnHandsUseCallback> ___callback_0)
        {
            if (CancelRequested)
            {
                __instance.ClearQueue();
                return true;
            }

#if DEBUG
            CH_Plugin.CH_Logger.LogWarning($"Effect is: {effect.GetType()}, Item is: {___medsController_0.Item.GetType()}]");
#endif
            if (effect is not GInterface350)
            {
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning("Was not a MedEffect! Ignoring...");
#endif
                return false;
            }

#if DEBUG
            if (effect is ActiveHealthController.GClass2813 durEffect)
            {
                CH_Plugin.CH_Logger.LogWarning("It's a durEffect, delay: " + durEffect.DelayTime);
            }
#endif

            Player player = (Player)playerField.GetValue(___medsController_0);
            if (player == null)
            {
                return true;
            }

            if (!player.IsYourPlayer)
            {
                return true;
            }

            if (___medsController_0.Item is not MedKitItemClass && (!CH_Plugin.HealLimbs.Value || ___medsController_0.Item is not MedicalItemClass))
            {
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning($"Item was not of MedKitItemClass/MedicalItemClass type, was: {___medsController_0.Item.GetType()}");
#endif
                return true;
            }

            MedsItemClass medsItem = (MedsItemClass)___medsController_0.Item;
            if (medsItem == null)
            {
                CH_Plugin.CH_Logger.LogError("medsItem was null!");
                return true;
            }

            if (medsItem.MedKitComponent == null)
            {
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning("MedKitComponent was null! Probably a single-use...");
#endif
                return true;
            }

            if (medsItem.MedKitComponent.HpResource <= 1 && medsItem.MedKitComponent.MaxHpResource < 95)
            {
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning("Resource was equalTo or lessThan 1 and not a healing kit, skipping...");
#endif
                return true;
            }

            if (player.ActiveHealthController.CanApplyItem(___medsController_0.Item, EBodyPart.Common))
            {
#if DEBUG
                CH_Plugin.CH_Logger.LogWarning("Can apply again!");
#endif
                player.HealthController.EffectRemovedEvent -= __instance.method_8;
                float originalDelay = ActiveHealthController.GClass2813.GClass2823_0.MedEffect.MedKitStartDelay;
                ActiveHealthController.GClass2813.GClass2823_0.MedEffect.MedKitStartDelay = (float)CH_Plugin.HealDelay.Value;
                IEffect newEffect = player.ActiveHealthController.DoMedEffect(___medsController_0.Item, EBodyPart.Common, 1f);
                if (newEffect == null)
                {
                    __instance.State = Player.EOperationState.Finished;
                    ___medsController_0.FailedToApply = true;
                    Callback<IOnHandsUseCallback> callbackToRun = ___callback_0;
                    ___callback_0 = null;
                    callbackToRun(___medsController_0);
                    ActiveHealthController.GClass2813.GClass2823_0.MedEffect.MedKitStartDelay = originalDelay;
                    return false;
                }
                ;
                player.HealthController.EffectRemovedEvent += __instance.method_8;
                ActiveHealthController.GClass2813.GClass2823_0.MedEffect.MedKitStartDelay = originalDelay;

                if (CH_Plugin.ResetAnimation.Value && ___medsController_0.Item is not MedicalItemClass)
                {
                    Animation++;
                    int variant = 0;
                    if (___medsController_0.Item.TryGetItemComponent(out AnimationVariantsComponent animationVariantsComponent))
                    {
                        variant = animationVariantsComponent.VariantsNumber;
                    }

                    int newAnim = (int)Mathf.Repeat((float)Animation, (float)variant);

                    if (___medsController_0.FirearmsAnimator != null)
                    {
                        float mult = player.Skills.SurgerySpeed.Value / 100f;
                        FirearmsAnimator animator = ___medsController_0.FirearmsAnimator;
                        animator.SetUseTimeMultiplier(1f + mult);
                        animator.SetActiveParam(true, false);
                        if (animator.HasNextLimb())
                        {
#if DEBUG
                            CH_Plugin.CH_Logger.LogWarning("Has next limb!");
#endif
                            animator.SetNextLimb(true);
                        }
#if DEBUG
                        CH_Plugin.CH_Logger.LogWarning("Setting new anim");
#endif
                        animator.SetAnimationVariant(newAnim);
                    }
                }

                return false;
            }

            return true;
        }
    }
}

using ProtoBuf;
using System;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.GameContent;
using System.Linq;
using System.Collections.Generic;
using Vintagestory.Server;
using Vintagestory.API.MathTools;
using Vintagestory.Client.NoObf;

[assembly: ModInfo("Subtitles")]

namespace Subtitles {
  public class SubtitlesMod : ModSystem {
    private Harmony harmony;
    private GuiDialogSubtitles dialog;
    public override void StartClientSide(ICoreClientAPI capi) {
      base.StartClientSide(capi);
      capi.Logger.Debug("Subtitles - applying patches");
      harmony = new Harmony("goxmeor.Subtitles");
      harmony.PatchAll();
      capi.Logger.Debug("Subtitles - patches applied");

      Patch_ClientMain_PlaySoundAtInternal.capi = capi;
      Patch_ClientMain_PlaySoundAtInternal.subtitlesMod = this;

      // using IsPlayerReady just like HudClock does
      capi.Event.IsPlayerReady += (ref EnumHandling handling) => {
        capi.Logger.Debug("Subtitles - creating and opening dialog");
        dialog = new GuiDialogSubtitles(capi, this);
        dialog.TryOpen();
        return true;
      };
    }
    public void OnNewSound(string name, double yaw, double distance) {
      dialog.subtitleElement.OnNewSound(name, yaw, distance);
    }
  }

  [HarmonyPatch(typeof(ClientMain))]
  [HarmonyPatch("PlaySoundAtInternal")]
  public class Patch_ClientMain_PlaySoundAtInternal {
    public static ICoreClientAPI capi;
    public static SubtitlesMod subtitlesMod;
    public static void Prefix(
      AssetLocation location,
      double x,
      double y,
      double z,
      float volume,
      float pitch,
      float range,
      EnumSoundType soundType
    ) {
      var player = capi.World.Player;
      var playerPos = player.Entity.Pos.AsBlockPos;

      var name = Lang.GetIfExists("subtitles:" + location.GetName());
      if (name == "") { return; }
      if (name == null) { name = location.GetName(); }

      if (x != 0 || y != 0 || z != 0) {
        capi.Logger.Debug($"{location.GetName()} {volume} {range} {soundType.ToString()} DIRECTIONAL");
        var dx = x - playerPos.X;
        var dy = y - playerPos.Y;
        var dz = z - playerPos.Z;

        if (Math.Abs(dx) < 1.5 && Math.Abs(dy) < 1.5 && Math.Abs(dz) < 1.5) {
          subtitlesMod.OnNewSound(name, Double.NaN, 0);
        }
        else {

          var yaw = Math.Atan2(dz, dx);
          // var direction = GameMath.Mod(angle / GameMath.TWOPI * 12, 12); // treat angles as 12 hour clock for easier mathing
          var distSqr = dx * dx + dy * dy + dz * dz;

          subtitlesMod.OnNewSound(name, yaw, Math.Max(24, range) / Math.Max(1, Math.Sqrt(distSqr)));

          // string leftSide = "  ";
          // string rightSide = "  ";
          // if (direction > 2 && direction < 4) { rightSide = ">>"; }
          // else if (direction > 1 && direction < 5) { rightSide = "> "; }
          // else if (direction > 8 && direction < 10) { leftSide = "<<"; }
          // else if (direction > 7 && direction < 11) { leftSide = " <"; }
          // capi.Logger.Debug($"{leftSide} {location.GetName()} {rightSide} ------ {angle} .. {Math.Sqrt(distSqr)} range {range} vol {volume}");
        }
      }
      else {
        capi.Logger.Debug($"{location.GetName()} {volume} {range} {soundType.ToString()} NON_DIR");
        subtitlesMod.OnNewSound(name, Double.NaN, 0);
        // capi.Logger.Debug($"{location.GetName()}");
      }
    }
  }
}

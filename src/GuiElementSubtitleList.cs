using Vintagestory.API.Client;
using Cairo;
using System.Collections.Generic;
using Vintagestory.API.MathTools;
using System;

namespace Subtitles {
  public class GuiElementSubtitleList : GuiElement {
    private LoadedTexture textTexture;
    private TextDrawUtil textUtil;
    public CairoFont Font;

    private static readonly int MAX_SOUNDS = 15;
    private static readonly int MAX_AGE_SECONDS = 3;
    public class Sound {
      public bool active = false;
      public double age;
      public string name;
      public double textWidth;
      public double yaw;
      public double volume;
    }
    public Sound[] soundList = new Sound[MAX_SOUNDS];

    public GuiElementSubtitleList(ICoreClientAPI capi, ElementBounds bounds)
      : base(capi, bounds) {
      textTexture = new LoadedTexture(capi);
      Font = CairoFont.WhiteSmallText();
      textUtil = new TextDrawUtil();
      for (int i = 0; i < MAX_SOUNDS; i++) { soundList[i] = new Sound(); }
    }

    public override void Dispose() {
      textTexture?.Dispose();
    }

    public override void ComposeElements(Context ctx, ImageSurface surface) {
      Font.SetupContext(ctx);
      Bounds.CalcWorldBounds();
      Recompose();
    }

    public override void RenderInteractiveElements(float deltaTime) {
      Update(deltaTime);
      Recompose();
      api.Render.Render2DTexturePremultipliedAlpha(textTexture.TextureId, (int)Bounds.renderX, (int)Bounds.renderY, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
    }

    public void Recompose() {
      ImageSurface imageSurface = new ImageSurface(Format.Argb32, (int)Bounds.InnerWidth, (int)Bounds.InnerHeight);
      Context context = genContext(imageSurface);
      DrawText(context);
      generateTexture(imageSurface, ref textTexture);
      context.Dispose();
      imageSurface.Dispose();
    }

    private void Update(float deltaTime) {
      foreach (var sound in soundList) {
        if (sound.active) {
          sound.age += deltaTime;
          if (sound.age >= MAX_AGE_SECONDS) { sound.active = false; }
        }
      }
    }
    private void DrawText(Context ctx) {
      Font.SetupContext(ctx);

      var y = 30 * (MAX_SOUNDS - 1);
      foreach (var sound in soundList) {
        if (sound.active) {

          if (sound.textWidth == -1) {
            sound.textWidth = ctx.TextExtents(sound.name).Width;
          }

          var brightness = ((1 - (sound.age / MAX_AGE_SECONDS)) * Math.Max(1, sound.volume) / 2 + 0.5);

          ctx.SetSourceRGBA(0, 0, 0, 0.25 + (brightness / 2));
          ctx.Rectangle(0, y, 250, 30);
          ctx.Fill();

          ctx.SetSourceRGB(brightness, brightness, brightness);
          textUtil.DrawTextLine(ctx, Font, sound.name, 125 - sound.textWidth / 2, y);

          if (!Double.IsNaN(sound.yaw)) {
            ctx.Save();
            Matrix matrix = ctx.Matrix;
            matrix.Translate(15, y + 16);
            matrix.Rotate(sound.yaw + api.World.Player.CameraYaw + Math.PI / 2);
            ctx.Matrix = matrix;
            textUtil.DrawTextLine(ctx, Font, "<", -10, -10);
            ctx.Restore();
            ctx.Save();
            matrix = ctx.Matrix;
            matrix.Translate(236, y + 16);
            matrix.Rotate(sound.yaw + api.World.Player.CameraYaw + Math.PI / 2);
            ctx.Matrix = matrix;
            textUtil.DrawTextLine(ctx, Font, "<", -10, -10);
            ctx.Restore();
          }


          // var direction = GameMath.Mod((sound.yaw + api.World.Player.CameraYaw) / GameMath.TWOPI * 12, 12);
          // if (direction > 2 && direction < 4) { textUtil.DrawTextLine(ctx, Font, ">>", 224, y); }
          // else if (direction > 1 && direction < 3) { textUtil.DrawTextLine(ctx, Font, ">", 224, y); }
          // else if (direction > 3 && direction < 5) { textUtil.DrawTextLine(ctx, Font, ">", 224, y); }
          // else if (direction > 8 && direction < 10) { textUtil.DrawTextLine(ctx, Font, "<<", 5, y); }
          // else if (direction > 7 && direction < 11) { textUtil.DrawTextLine(ctx, Font, "<", 17, y); }
        }

        y -= 30;
      }
    }

    public void OnNewSound(string name, double yaw, double distance) {
      Sound targetSound = null;
      foreach (var sound in soundList) {
        if (sound.active && sound.name == name) {
          targetSound = sound;
          break;
        }
      }
      if (targetSound == null) {
        targetSound = soundList[0];
        foreach (var sound in soundList) {
          if (!sound.active) {
            targetSound = sound;
            break;
          }
          if (sound.age > targetSound.age) { targetSound = sound; }
        }
      }
      targetSound.active = true;
      targetSound.name = name;
      targetSound.age = 0;
      targetSound.yaw = yaw;
      targetSound.volume = distance;
      targetSound.textWidth = -1;
    }
  }
}

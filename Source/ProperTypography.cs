using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using HarmonyLib;
using System.Reflection.Emit;
namespace ProperTypography
{
  [StaticConstructorOnStartup]
  public static class ProperTypography
  {
    static ProperTypography()
    {
      var harmony = new Harmony("ProperTypography");
      harmony.PatchAll(Assembly.GetExecutingAssembly());
      Harmony.DEBUG = true;
    }


    [HarmonyPatch(typeof(Verse.IntRange), "FromString")]
    public static class IntRange_FromString_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string s)
      {
        // The original method is very rigid in terms of the necessary string format to work correctly, so the string with proper typography has to be reverted to match the restrictions.
        s = s.Replace("ProperTypography_Range".Translate(), "~").Replace("ProperTypography_Minus".Translate(), "-");
      }
    }

    [HarmonyPatch(typeof(Verse.FloatRange), "FromString")]
    public static class FloatRange_FromString_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string s)
      {
        // The original method is very rigid in terms of the necessary string format to work correctly, so the string with proper typography has to be reverted to match the restrictions.
        s = s.Replace("ProperTypography_Range".Translate(), "~").Replace("ProperTypography_Minus".Translate(), "-");
      }
    }

    

    [HarmonyPatch(typeof(Verse.GenText), "ToStringTemperature")]
    public static class GenText_ToStringTemperature_Patch
    {
      [HarmonyPrefix]
      public static bool Prefix(ref string __result, ref float celsiusTemp, string format = "F1")
      {
        float temp = GenTemperature.CelsiusTo(celsiusTemp, Prefs.TemperatureMode);
        switch (Prefs.TemperatureMode)
        {
          case TemperatureDisplayMode.Celsius:
            __result = "ProperTypography_Celsius_Single".Translate(temp.ToString(format));
            break;
          case TemperatureDisplayMode.Fahrenheit:
            __result = "ProperTypography_Fahrenheit_Single".Translate(temp.ToString(format));
            break;
          case TemperatureDisplayMode.Kelvin:
            __result = "ProperTypography_Kelvin_Single".Translate(temp.ToString(format));
            break;
          default:
            throw new InvalidOperationException();
        }
        // Replace minus signs (if any).
        __result = __result.Replace("-", TranslatorFormattedStringExtensions.Translate("ProperTypography_Minus"));
        return false;
      }
    }

   
    [HarmonyPatch(typeof(Verse.GenText), "ToStringTemperatureOffset")]
    public static class GenText_ToStringTemperatureOffset_Patch
    {
      [HarmonyPrefix]
      public static bool Prefix(ref string __result, ref float celsiusTemp, string format = "F1")
      {
        float temp = GenTemperature.CelsiusToOffset(celsiusTemp, Prefs.TemperatureMode);
        switch (Prefs.TemperatureMode)
        {
          case TemperatureDisplayMode.Celsius:
            __result = "ProperTypography_Celsius_Single".Translate(temp.ToString(format));
            break;
          case TemperatureDisplayMode.Fahrenheit:
            __result = "ProperTypography_Fahrenheit_Single".Translate(temp.ToString(format));
            break;
          case TemperatureDisplayMode.Kelvin:
            __result = "ProperTypography_Kelvin_Single".Translate(temp.ToString(format));
            break;
          default:
            throw new InvalidOperationException();
        }
        // Replace minus signs (if any).
        __result = __result.Replace("-", TranslatorFormattedStringExtensions.Translate("ProperTypography_Minus"));
        return false;
      }
    }



    // #################################### //
    // Patching average temperature ranges. //
    // #################################### //

    [HarmonyPatch(typeof(Verse.GenTemperature), "GetAverageTemperatureLabel")]
    public static class GenTemperature_GetAverageTemperatureLabel_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result, ref int tile)
      {
        __result = Find.WorldGrid[tile].temperature.ToStringTemperature() + " " + string.Format("({0} {1} {2})", (GenTemperature.CelsiusTo(GenTemperature.MinTemperatureAtTile(tile), Prefs.TemperatureMode)).ToString("F0").Replace("-", "ProperTypography_Minus".Translate()), "RangeTo".Translate(), GenTemperature.MaxTemperatureAtTile(tile).ToStringTemperature("F0"));
      }
    }


    // ############################ //
    // Patching temperature format. //
    // ############################ //

    [HarmonyPatch(typeof(RimWorld.ITab_Pawn_Gear))]
    [HarmonyPatch("TryDrawComfyTemperatureRange")]
    public static class RimWorld_ITab_Pawn_Gear_TryDrawComfyTemperatureRange_Patch
    {
      [HarmonyPrefix]
      public static bool Prefix(ITab_Pawn_Gear __instance, ref float curY, float width)
      {
        Pawn SelPawnForGear = Traverse.Create(__instance).Property("SelPawnForGear").GetValue<Pawn>();
        if (!SelPawnForGear.Dead)
        {
          Rect rect = new Rect(0f, curY, width, 22f);
          float statValue = SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMin);
          float statValue2 = SelPawnForGear.GetStatValue(StatDefOf.ComfyTemperatureMax);
          string labelText = "";
          switch (Prefs.TemperatureMode)
          {
            case TemperatureDisplayMode.Celsius:
              labelText = "ComfyTemperatureRange".Translate() + ": " + TranslatorFormattedStringExtensions.Translate("ProperTypography_Celsius_Range",statValue.ToString("F0"),  statValue2.ToString("F0"));
              break;
            case TemperatureDisplayMode.Fahrenheit:
              labelText = "ComfyTemperatureRange".Translate() + ": " + TranslatorFormattedStringExtensions.Translate("ProperTypography_Fahrenheit_Range",statValue.ToString("F0"),  statValue2.ToString("F0"));
              break;
            case TemperatureDisplayMode.Kelvin:
              labelText = "ComfyTemperatureRange".Translate() + ": " + TranslatorFormattedStringExtensions.Translate("ProperTypography_Kelvin_Range",statValue.ToString("F0"),  statValue2.ToString("F0"));
              break;
            default:
              throw new InvalidOperationException();
          }
          // Replace minus signs (if any).
          labelText = labelText.Replace("-", TranslatorFormattedStringExtensions.Translate("ProperTypography_Minus"));
          Widgets.Label(rect, labelText);
          curY += 22f;
        }
        return false;
      }
    }

    // The ToStringPercent method exists twice with two different sets of parameters, so both must be patched to get all occurances of percentage strings.
    [HarmonyPatch(typeof(Verse.GenText), "ToStringPercent", new Type[] {typeof(float)})]
    public static class GenText_ToStringPercent_Patch_1
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = TranslatorFormattedStringExtensions.Translate("ProperTypography_Percentage", __result.Replace("%", ""));
      }
    }

    [HarmonyPatch(typeof(Verse.GenText), "ToStringPercent", new Type[] {typeof(float), typeof(string)})]
    public static class GenText_ToStringPercent_Patch_2
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = TranslatorFormattedStringExtensions.Translate("ProperTypography_Percentage", __result.Replace("%", ""));
      }
    }

    [HarmonyPatch(typeof(Verse.Widgets))]
    [HarmonyPatch("TextFieldPercent")]
    public static class Widgets_TextFieldPercent_Patch
    {
      [HarmonyPrefix]
      public static bool Prefix(Rect rect, ref float val, ref string buffer, float min = 0f, float max = 1f)
      {
        Rect rect2 = new Rect(rect.x, rect.y, rect.width - 25f, rect.height);
        Widgets.Label(new Rect(rect2.xMax, rect.y, 25f, rect2.height), TranslatorFormattedStringExtensions.Translate("ProperTypography_Percentage", ""));
        float val2 = val * 100f;
        Widgets.TextFieldNumeric(rect2, ref val2, ref buffer, min * 100f, max * 100f);
        val = val2 / 100f;
        if (val > max)
        {
          val = max;
          buffer = val.ToString();
        }
        return false;
      }
    }



    // ######################################################## //
    // Patching geographical coordinates in the world map view. //
    // ######################################################## //

    [HarmonyPatch(typeof(Verse.GenText), "ToStringLatitude")]
    public static class GenText_ToStringLatitude_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        if (__result != null)
        {
          Regex rg = new Regex("(°)([NSWE])");
          Match coord = rg.Match(__result);
          if (coord.Groups.Count == 3)
          {
            __result = __result.Replace(coord.Groups[1].Value + coord.Groups[2].Value, TranslatorFormattedStringExtensions.Translate("ProperTypography_GeoCoord", coord.Groups[1].Value, coord.Groups[2].Value));
          }
          else
          {
            Log.Message("Failed to patch coordinates.");
          }
        }
      }
    }


    [HarmonyPatch(typeof(Verse.GenText), "ToStringLongitude")]
    public static class GenText_ToStringLongitude_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        if (__result != null)
        {
          Regex rg = new Regex("(°)([NSWE])");
          Match coord = rg.Match(__result);
          if (coord.Groups.Count == 3)
          {
            __result = __result.Replace(coord.Groups[1].Value + coord.Groups[2].Value, TranslatorFormattedStringExtensions.Translate("ProperTypography_GeoCoord", coord.Groups[1].Value, coord.Groups[2].Value));
          }
          else
          {
            Log.Message("Failed to patch coordinates.");
          }
        }
      }
    }



    // ############################################# //
    // Patching various units in the world map view. //
    // ############################################# //

    [HarmonyPatch(typeof(Verse.Listing_Standard), "LabelDouble")]
    public class Listing_Standard_LabelDouble_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string leftLabel, ref string rightLabel)
      {
        Regex rg;
        if (leftLabel == "Elevation".Translate())
        {
          rg = new Regex("([^m]+)");
          Match valMatch = rg.Match(rightLabel);
          string val = valMatch.Groups[0].Value.Replace("-", TranslatorFormattedStringExtensions.Translate("ProperTypography_Minus"));
          rightLabel = TranslatorFormattedStringExtensions.Translate("ProperTypography_Meters", val);
        }
        else if (leftLabel == "Rainfall".Translate())
        {
          rg = new Regex("([^m]+)");
          Match valMatch = rg.Match(rightLabel);
          string val = valMatch.Groups[0].Value.Replace("-", TranslatorFormattedStringExtensions.Translate("ProperTypography_Minus"));
          rightLabel = TranslatorFormattedStringExtensions.Translate("ProperTypography_Millimeters", val);
        }
        else if (leftLabel == "OutdoorGrowingPeriod".Translate())
        {
          rightLabel = rightLabel.Replace(" - ", TranslatorFormattedStringExtensions.Translate("ProperTypography_Range"));
        }
      }
    }



    [HarmonyPatch(typeof(System.Text.StringBuilder), "Append", new Type[] {typeof(string)})]
    public class StringBuilder_Append_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string value)
      {
        if (value == "  - ")
          value = "ProperTypography_Enumeration_lvl0".Translate();
      }
    }



    [HarmonyPatch(typeof(System.Text.StringBuilder), "AppendLine", new Type[] {typeof(string)})]
    public class AppendLine_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string value)
      {
        value = Regex.Replace(value, @"^ {4}([^ ]+)", "ProperTypography_Enumeration_lvl0".Translate() + "$1"); // Replace with non-breaking spaces.
        value = Regex.Replace(value, @"-([0-9])", "ProperTypography_Minus".Translate() + "$1");
        value = Regex.Replace(value, @"([0-9]) / ([0-9])", "$1" + "ProperTypography_Division_XofY".Translate() + "$2");
      }
    }



    [HarmonyPatch(typeof(Verse.ColoredText), "AppendLineTagged")]
    public class ColoredText_AppendLineTagged_Patch
    {
      [HarmonyPrefix]
      public static bool Prefix(ref TaggedString taggedString)
      {
        if (taggedString.ToString().Contains("SkillLevel".Translate().CapitalizeFirst()))
        {
          Regex rg = new Regex("(.*?)(" + "SkillLevel".Translate().CapitalizeFirst() + ":)( </color>[^0-9]*)([0-9]+) - ([^\n]+)");
          Match skillLevel = rg.Match(taggedString);
          if (skillLevel.Groups.Count == 6)
          {
            // To do: Restore the colors. But how?
            taggedString = new TaggedString((skillLevel.Groups[1].Value + skillLevel.Groups[2].Value).AsTipTitle() + skillLevel.Groups[3].Value + skillLevel.Groups[4].Value + " (" + skillLevel.Groups[5].Value + ")");
          }
          return false;
        }
        else
        {
          return true;
        }
      }
    }



    [HarmonyPatch(typeof(Verse.GenText), "AppendInNewLine")]
    public class GenText_AppendInNewLine_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string text)
      {
        text = Regex.Replace(text, @"^- ", "ProperTypography_Enumeration_lvl0".Translate());
      }
    }



    [HarmonyPatch(typeof(RimWorld.Pawn_RelationsTracker), "OpinionExplanation")]
    public class Pawn_RelationsTracker_OpinionExplanation_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"\n - ", "\n" + "ProperTypography_Enumeration_lvl0".Translate());
        __result = Regex.Replace(__result, @"-([0-9])", "ProperTypography_Minus".Translate() + "$1");
      }
    }



    [HarmonyPatch(typeof(RimWorld.TransferableOneWayWidget), "GetPawnMassTip")]
    public class TransferableOneWayWidget_GetPawnMassTip_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"\n - ", "\n" + "ProperTypography_Enumeration_lvl0".Translate());
        __result = Regex.Replace(__result, @"-([0-9])", "ProperTypography_Minus".Translate() + "$1");
      }
    }



    [HarmonyPatch(typeof(RimWorld.Need_Food), "GetTipString")]
    public class Need_Food_GetTipString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace(" / ", "ProperTypography_Division_XofY".Translate());
      }
    }
    


    // ################################################################# //
    // Disabled work types list with user-defined enumeration character. //
    // ################################################################# //

    [HarmonyPatch(typeof(RimWorld.CharacterCardUtility), "GetWorkTypesDisabledByWorkTag")]
    public class CharacterCardUtility_GetWorkTypesDisabledByWorkTag
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"\n- ", "\n" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Enumeration_lvl0"));
      }
    }

    
    // Fixing some more lists and unifying the list format for all occuring lists.
    [HarmonyPatch(typeof(Verse.GenText), "ToLineList", new Type[] {typeof(IList<string>), typeof(string)})]
    public class GenText_ToLineList_Patch_0
    {
      [HarmonyPrefix]
      public static void Prefix(ref string prefix)
      {
        Match match = Regex.Match(prefix, "^ *- *$");
        if (match.Success)
          prefix = TranslatorFormattedStringExtensions.Translate("ProperTypography_Enumeration_lvl0");
      }
    }

    [HarmonyPatch(typeof(Verse.GenText), "ToLineList", new Type[] {typeof(IList<string>), typeof(string), typeof(bool)})]
    public class GenText_ToLineList_Patch_1
    {
      [HarmonyPrefix]
      public static void Prefix(ref string prefix)
      {
        Match match = Regex.Match(prefix, "^ *- *$");
        if (match.Success)
          prefix = TranslatorFormattedStringExtensions.Translate("ProperTypography_Enumeration_lvl0");
      }
    }



    // ################################################################################################################# //
    // Summary when starting a scenario. Patching the syntax for quantaties of starting resources and their enumeration. //
    // ################################################################################################################# //

    [HarmonyPatch(typeof(RimWorld.ScenSummaryList), "SummaryList")]
    public class ScenSummaryList_SummaryList_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace("\n" + __result, @"\n   -", "\n" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Enumeration_lvl0")).Substring(1);
        __result = Regex.Replace(__result, @"x([0-9]+)", TranslatorFormattedStringExtensions.Translate("ProperTypography_Multiplication_NumberRight") + "$1");
      }
    }


    // Properly enumerate traits affecting the character’s physical capabilities and vital functions.
    [HarmonyPatch(typeof(RimWorld.HealthCardUtility), "GetPawnCapacityTip", new Type[] {typeof(Pawn), typeof(PawnCapacityDef)})]
    public class HealthCardUtility_GetPawnCapacityTip_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"\n  ", "\n" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Enumeration_lvl0"));
        __result = Regex.Replace(__result, @"([0-9])%", "$1" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Percentage", ""));
      }
    }


    // Correct pain level percentage typography.
    [HarmonyPatch(typeof(RimWorld.HealthCardUtility), "GetPainTip", new Type[] {typeof(Pawn)})]
    public class HealthCardUtility_GetPainTip_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"([0-9])%", "$1" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Percentage", ""));
      }
    }


    [HarmonyPatch(typeof(Verse.GenText), "ToStringWithSign", new Type[] {typeof(int)})]
    public class GenText_ToStringWithSign_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace("-", "ProperTypography_Minus".Translate());
      }
    }


    [HarmonyPatch(typeof(Verse.GenText), "ToStringWithSign", new Type[] {typeof(float), typeof(string)})]
    public class GenText_ToStringWithSign_Patch_1
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace("-", "ProperTypography_Minus".Translate());
      }
    }



    
    // ################### //
    // Inventory tooltips. //
    // ################### //

    [HarmonyPatch(typeof(Verse.TooltipHandler), "TipRegion", new Type[] {typeof(Rect), typeof(TipSignal)})]
    public class TooltipHandler_TipRegion_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref TipSignal tip)
      {
        tip.text = Regex.Match(tip.text, "[0-9]+ / [0-9]+").Success ? tip.text.Replace(" / ", "ProperTypography_Division_XofY".Translate()) : tip.text;
      }
    }



    // ################################################# //
    // Replace the arrows in the work assignment dialog. //
    // ################################################# //

    [HarmonyPatch(typeof(Verse.Widgets), "Label", new Type[] {typeof(Rect), typeof(string)})]
    public class Widgets_Label_Patch_0
    {
      [HarmonyPrefix]
      public static void Prefix(ref string label)
      {
        label = label.Contains("<= ") ? label.Replace("<= ", "⇐ ") : label;
        label = label.Contains(" =>") ? label.Replace(" =>", " ⇒") : label;
        label = label == "-" ? "—" : label;
        label = Regex.Match(label, "^ ?-[0-9.]+").Success ? label.Replace("-", "ProperTypography_Minus".Translate()) : label;
        label = Regex.Match(label, @"^\(-[0-9.]+\)$").Success ? label.Replace("-", "ProperTypography_Minus".Translate()) : label;
        label = Regex.Match(label, "^" + "BleedingRate".Translate() + ": ").Success ? label.Replace("/" + "LetterDay".Translate(), "ProperTypography_Division_Units".Translate() + "LetterDay".Translate()) : label;
        label = Regex.Match(label, "z/s$").Success ? label.Replace("z/s", "z" + "ProperTypography_Division_Units".Translate() + "s") : label;
        label = Regex.Match(label, "^-?[0-9]+%$").Success ? label.Replace("-", "ProperTypography_Minus".Translate()).Replace("%", "ProperTypography_Percentage".Translate("")) : label;
        label = Regex.Match(label, "^[0-9]+ / [0-9]+$").Success ? label.Replace(" / ", "ProperTypography_Division_XofY".Translate()) : label;
        label = Regex.Match(label, "[0-9.]+ / [0-9.]+ k?g").Success ? label.Replace(" / ", "ProperTypography_Division_XofY".Translate()) : label;
        label = Regex.Replace(label, @"([0-9.]+)" + "ProperTypography_Percentage".Translate("") + " - ([0-9.]+)" + "ProperTypography_Percentage".Translate(""), "$1" + "ProperTypography_Range".Translate("") + "$2" + "ProperTypography_Percentage".Translate(""));
        label = Regex.Replace(label, @" x([0-9])", " " + "ProperTypography_Multiplication_NumberRight".Translate() + "$1");
      }
    }



    [HarmonyPatch(typeof(RimWorld.Zone_Growing), "GetInspectString")]
    public class Zone_Growing_GetInspectString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, "([0-9])/([0-9])", "$1" + "ProperTypography_Division_XofY".Translate() + "$2");
        __result = __result.Replace(" - ", "ProperTypography_Range".Translate());
      }
    }



    [HarmonyPatch(typeof(RimWorld.Frame), "GetInspectString")]
    public class Frame_GetInspectString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, "([0-9]) / ([0-9])", "$1" + "ProperTypography_Division_XofY".Translate() + "$2");
      }
    }


    [HarmonyPatch(typeof(RimWorld.Blueprint_Build), "GetInspectString")]
    public class Blueprint_Build_GetInspectString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, "([0-9]) / ([0-9])", "$1" + "ProperTypography_Division_XofY".Translate() + "$2");
      }
    }



    [HarmonyPatch(typeof(RimWorld.PawnColumnWorker_WorkPriority), "SpecificWorkListString")]
    public class PawnColumnWorker_WorkPriority_SpecificWorkListString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace(" - ", TranslatorFormattedStringExtensions.Translate("ProperTypography_Enumeration_lvl0"));
      }
    }



    // ##################################################### //
    // Change the percentage syntax for terrain light level. //
    // ##################################################### //

    [HarmonyPatch(typeof(Verse.MouseoverUtility))]
    [HarmonyPatch("GetGlowLabelByValue")]
    public static class MouseoverUtility_GetGlowLabelByValue_Patch
    {
      [HarmonyPostfix]
      private static void Postfix(ref string __result)
      {
        __result = __result.Replace("%", TranslatorFormattedStringExtensions.Translate("ProperTypography_Percentage", ""));
      }
    }



    // ###################################### //
    // Some replacements in the stats report. //
    // ###################################### //

    [HarmonyPatch(typeof(RimWorld.StatWorker), "GetExplanationFull", new Type[] {typeof(StatRequest), typeof(ToStringNumberSense), typeof(float)})]
    public static class StatWorker_GetExplanationFull_Patch
    {
      [HarmonyPostfix]
      private static void Postfix(ref string __result)
      {
        //Log.Message("VOLLSTÄNDIG: " + __result);
        __result = Regex.Replace(__result, @"-([0-9])", "ProperTypography_Minus".Translate() + "$1");
        __result = Regex.Replace(__result, @"([0-9]) x ([0-9])", "$1" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Multiplication_TwoNumbers") + "$2");
        __result = Regex.Replace(__result, @"([0-9]) ?x", "$1" + TranslatorFormattedStringExtensions.Translate("ProperTypography_Multiplication_NumberLeft"));
        __result = Regex.Replace(__result, @"([a-z:]) x ?([0-9])", "$1" + " " + TranslatorFormattedStringExtensions.Translate("ProperTypography_Multiplication_NumberRight") + "$2");
      }
    }



    // ##################################################################### //
    // Change the quotations marks for nicknames in the character info card. //
    // ##################################################################### //
    // ToStringFull is not a method, but a so-called property, as it contains a get block and takes no arguments. Prefix the name with »get_« to fetch the according method.

    [HarmonyPatch(typeof(Verse.NameTriple), "get_ToStringFull", new Type[] {})]
    public static class NameTriple_ToString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace(" '", " " + TranslatorFormattedStringExtensions.Translate("ProperTypography_QuotationOpening")).Replace("' ", TranslatorFormattedStringExtensions.Translate("ProperTypography_QuotationClosing") + " ");
      }
    }


    
    // #################################################################################################### //
    // This patch is needed to prevent the Compare method from breaking due to changes in the units syntax. //
    // #################################################################################################### //
    // The original Compare method is very fragile as it expects a certain number/range format.
    // This prefix method fixes that by stripping units and selecting the first value of ranges beforehand.

    [HarmonyPatch(typeof(Verse.NumericStringComparer), "Compare", new Type[] {typeof(string), typeof(string)})]
    public class NumericStringComparer_Compare_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref string x, ref string y)
      {
        Regex rg = new Regex("[^a-z0-9+]?[0-9.]+");
        MatchCollection numbers_x = rg.Matches(x);
        MatchCollection numbers_y = rg.Matches(y);
      
        x = numbers_x.Count > 0 ? numbers_x[0].Value : "";
        y = numbers_y.Count > 0 ? numbers_y[0].Value : "";

        // In case of negative values and replaced signum for negative numbers, the negative value must be restored.
        if (Regex.Replace(x, @"[0-9.]+", "").Length > 0 && x.Length > 0)
          x = "-" + Regex.Match(x, "[0-9.]+");
  
        if (Regex.Replace(y, @"[0-9.]+", "").Length > 0 && y.Length > 0)
          y = "-" + Regex.Match(y, "[0-9.]+");
      }
    }



    // #################### //
    // Fix range character. //
    // #################### //

    [HarmonyPatch(typeof(Verse.FloatRange), "ToString")]
    public class FloatRange_ToString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace("~", "ProperTypography_Range".Translate());
      }
    }

    [HarmonyPatch(typeof(Verse.IntRange), "ToString")]
    public class IntRange_ToString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace("~", "ProperTypography_Range".Translate());
      }
    }

    

    // ################################################################ //
    // Patching the skill description popup in the character’s bio tab. //
    // ################################################################ //

    [HarmonyPatch(typeof(RimWorld.SkillUI), "GetSkillDescription")]
    public class SkillUI_GetSkillDescription_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @">-([0-9.]+)", ">" + "ProperTypography_Minus".Translate() + "$1");
        __result = Regex.Replace(__result, @"\n  - ", "\n" + "ProperTypography_Enumeration_lvl0".Translate());
        __result = Regex.Replace(__result, @"^([^\n]+)\n", "$1\n\n");
        __result = Regex.Replace(__result, @"([a-z:]) x ?([0-9])", "$1" + " " + TranslatorFormattedStringExtensions.Translate("ProperTypography_Multiplication_NumberRight") + "$2");
        __result = Regex.Replace(__result, @"([0-9]) / ([0-9])", "$1" + "ProperTypography_Division_XofY".Translate() + "$2");
      }
    }



    // ############################################### //
    // Change the daytime format displayed in the GUI. //
    // ############################################### //

    [HarmonyPatch(typeof(RimWorld.DateReadout), "DateOnGUI", new Type[] {typeof(Rect)})]
    public class DateReadout_DateOnGUI_Patch
    {
      [HarmonyPrefix]
      public static void Prefix(ref List<string> ___fastHourStrings)
      {
        // Overwrite the whole list only once.
        if (___fastHourStrings[0] != "0" + "ProperTypography_TimeOfDay".Translate())
        {
          for (int i = 0; i < 24; i++)
          {
            ___fastHourStrings[i] = i + "ProperTypography_TimeOfDay".Translate();
            
          }
        }
      }
    }



    // ########################################################### //
    // Make MeleeWeapon_AverageDPS use the proper typography keys. //
    // ########################################################### //

    [HarmonyPatch(typeof(RimWorld.StatWorker_MeleeDPS), "GetStatDrawEntryLabel", new Type[] {typeof(StatDef), typeof(float), typeof(ToStringNumberSense), typeof(StatRequest), typeof(bool)})]
    public class StatWorker_MeleeDPS_GetStatDrawEntryLabel_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = __result.Replace(" ( ", " (").Replace(" x ", "ProperTypography_Multiplication_TwoNumbers".Translate()).Replace(" / ", "ProperTypography_Division_Operator".Translate()).Replace(" )", ")");
      }
    }



    // ##################################### //
    // Enumeration of Hediff/injury effects. //
    // ##################################### //

    [HarmonyPatch(typeof(Verse.Hediff), "GetTooltip")]
    public class Hediff_TipStringExtra_Patch
    {
      [HarmonyPostfix]
      private static void Postfix(ref string __result)
      {
        __result = __result.Replace("  - ", "ProperTypography_Enumeration_lvl0".Translate());
        __result = __result.Replace("%", "ProperTypography_Percentage".Translate(""));
        __result = Regex.Replace(__result, @"-([0-9])", "ProperTypography_Minus".Translate() + "$1");
        __result = Regex.Replace(__result, @"\n\n", "\n");
        __result = Regex.Replace(__result, @"\n([^\n ])", "\n\n$1");
        __result = Regex.Replace(__result, @"([a-z:]) x ?([0-9])", "$1" + " " + TranslatorFormattedStringExtensions.Translate("ProperTypography_Multiplication_NumberRight") + "$2");
        // Also patching the wrong term »Sleep fall rate«, which is not defined anywhere and equals »tiredness« according to the wiki.
        __result = __result.Replace("Sleep fall rate", "Tiredness".Translate());
      }
    }



    // ################################################### //
    // Patching the age output in the character’s bio tab. //
    // ################################################### //

    [HarmonyPatch(typeof(Verse.Pawn_AgeTracker), "get_AgeTooltipString")]
    public class Pawn_AgeTracker_AgeTooltipString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        // Get the first three lines of the __result string.
        MatchCollection ageInfo = Regex.Matches(__result, "([^\n]+)", RegexOptions.Multiline);
        string lineTwo = ageInfo[1].Value;
        string lineThree = ageInfo[2].Value;
        string age_chron = Regex.Match(lineTwo, @"^[^0-9]+").Value;
        string age_biol = Regex.Match(lineThree, @"^[^0-9]+").Value;

        ageInfo = Regex.Matches(__result, "([0-9]+)", RegexOptions.Multiline);
        int[] ageNumbers = {
          Int32.Parse(ageInfo[2].Value),
          Int32.Parse(ageInfo[3].Value),
          Int32.Parse(ageInfo[4].Value),
          Int32.Parse(ageInfo[5].Value),
          Int32.Parse(ageInfo[6].Value),
          Int32.Parse(ageInfo[7].Value)
        };
        if (ageNumbers[0] > 0)
          age_chron = age_chron + (ageNumbers[0] > 1 ? "PeriodYears" : "Period1Year").Translate(ageNumbers[0]);
        
        if (ageNumbers[1] > 0)
          age_chron = age_chron + (ageNumbers[0] > 0 ? ", " : "") + (ageNumbers[1] > 1 ? "PeriodQuadrums" : "Period1Quadrum").Translate(ageNumbers[1]);
        
        if (ageNumbers[2] > 0)
          age_chron = age_chron + (ageNumbers[0] > 0 || ageNumbers[1] > 0 ? ", " : "") + (ageNumbers[2] > 1 ? "PeriodDays" : "Period1Day").Translate(ageNumbers[2]);
        
        if (ageNumbers[3] > 0)
          age_biol = age_biol + (ageNumbers[3] > 1 ? "PeriodYears" : "Period1Year").Translate(ageNumbers[3]);
        
        if (ageNumbers[4] > 0)
          age_biol = age_biol + (ageNumbers[3] > 0 ? ", " : "") + (ageNumbers[4] > 1 ? "PeriodQuadrums" : "Period1Quadrum").Translate(ageNumbers[4]);
        
        if (ageNumbers[5] > 0)
          age_biol = age_biol + (ageNumbers[3] > 0 || ageNumbers[4] > 0 ? ", " : "") + (ageNumbers[5] > 1 ? "PeriodDays" : "Period1Day").Translate(ageNumbers[5]);
        
        __result = __result.Replace(lineTwo, age_chron);
        __result = __result.Replace(lineThree, age_biol);        
      }
    }
    


    [HarmonyPatch(typeof(Verse.GenText), "ToStringByStyle")]
    public class GenText_ToStringByStyle_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"x([0-9])", "ProperTypography_Multiplicator".Translate() + "$1");
			}
    }



    [HarmonyPatch(typeof(RimWorld.Dialog_Options), "ResToString")]
    public class Dialog_Options_ResToString_Patch
    {
      [HarmonyPostfix]
      public static void Postfix(ref string __result)
      {
        __result = Regex.Replace(__result, @"([0-9])x([0-9])", "$1" + "ProperTypography_Multiplication_TwoNumbers".Translate() + "$2");
      }
    }



    [HarmonyPatch(typeof(Verse.Listing_Standard), "ButtonTextLabeledPct")]
    public class Listing_Standard_ButtonTextLabeledPct
    {
      [HarmonyPrefix]
      public static void Prefix(ref string buttonLabel)
      {
        if (Regex.Match(buttonLabel, @"^[0-9.]+x$").Success)
        {
          buttonLabel = Regex.Replace(buttonLabel, @"([0-9.]+)x", "$1" + "ProperTypography_Multiplication_NumberLeft".Translate());
        }
      }
    }    
  }
}

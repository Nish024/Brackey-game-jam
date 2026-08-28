using UnityEngine;
using UnityEditor;

/// <summary>
/// Generates the 4 specialist DialogueData assets (Pistol, Rifle, Shotgun, Revolver)
/// using the DialogueData / DialogueNode / DialogueChoice schema.
///
/// SETUP:
/// 1. Place this script inside an "Editor" folder, e.g. Assets/Scripts/Editor/
///    (it uses UnityEditor, so it must live in a folder named "Editor").
/// 2. Make sure DialogueData.cs / DialogueNode / DialogueChoice already exist
///    in your project (non-Editor folder) matching the schema you shared.
/// 3. In Unity, go to the menu: Tools > Generate Specialist Dialogues
/// 4. Check Assets/Dialogues — you'll get 4 populated .asset files:
///    Pistol_EugeneStoner, Rifle_MikhailKalashnikov, Shotgun_JohnBrowning, Revolver_SamuelColt
///
/// Re-running the menu command updates the same assets in place instead of duplicating them,
/// so it's safe to tweak text in this script and re-generate.
/// </summary>
public class SpecialistDialogueGenerator
{
    private const string OutputFolder = "Assets/Dialogues";

    [MenuItem("Tools/Generate Specialist Dialogues")]
    public static void GenerateAll()
    {
        EnsureFolder();

        GeneratePistol();
        GenerateRifle();
        GenerateShotgun();
        GenerateRevolver();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Specialist dialogues generated in " + OutputFolder);
    }

    private static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder(OutputFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Dialogues");
        }
    }

    private static void SaveAsset(DialogueData data, string fileName)
    {
        string path = OutputFolder + "/" + fileName + ".asset";
        DialogueData existing = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

        if (existing != null)
        {
            // Update existing asset in place so scene references don't break
            existing.specialistName = data.specialistName;
            existing.callFee = data.callFee;
            existing.startNodeID = data.startNodeID;
            existing.nodes = data.nodes;
            EditorUtility.SetDirty(existing);
        }
        else
        {
            AssetDatabase.CreateAsset(data, path);
        }
    }

    // ---------------------------------------------------------
    // PISTOL — Eugene Stoner
    // ---------------------------------------------------------
    private static void GeneratePistol()
    {
        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.specialistName = "Eugene Stoner";
        data.callFee = 40;
        data.startNodeID = "start";

        data.nodes = new DialogueNode[]
        {
            new DialogueNode
            {
                nodeID = "start",
                speaker = Speaker.Player,
                text = "Got a pistol here, guy's asking $650. Says it's the real deal.",
                nextNodeID = "greet"
            },
            new DialogueNode
            {
                nodeID = "greet",
                speaker = Speaker.Specialist,
                text = "Alright, walk me through what you're seeing on it.",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "hub",
                speaker = Speaker.Player,
                choices = new DialogueChoice[]
                {
                    new DialogueChoice { choiceText = "It's got a logo on the handle.", nextNodeID = "logo_ask" },
                    new DialogueChoice { choiceText = "There's engraving on the slide.", nextNodeID = "engrave_ask" },
                    new DialogueChoice { choiceText = "Finish looks chrome or nickel.", nextNodeID = "chrome_info" },
                    new DialogueChoice { choiceText = "Grip panels are wood.", nextNodeID = "wood_info" },
                    new DialogueChoice { choiceText = "No, I think I've got what I need.", nextNodeID = "end" }
                }
            },
            new DialogueNode
            {
                nodeID = "logo_ask",
                speaker = Speaker.Specialist,
                text = "What's the logo — a bullet shape?",
                nextNodeID = "logo_choice"
            },
            new DialogueNode
            {
                nodeID = "logo_choice",
                speaker = Speaker.Player,
                choices = new DialogueChoice[]
                {
                    new DialogueChoice { choiceText = "Yeah, looks like a bullet.", nextNodeID = "logo_bullet_info" },
                    new DialogueChoice { choiceText = "Something else, not sure.", nextNodeID = "logo_other_info" }
                }
            },
            new DialogueNode
            {
                nodeID = "logo_bullet_info",
                speaker = Speaker.Specialist,
                text = "Then the handle should be red, that's factory spec for that line. Serial number should fall between 345 and 760.",
                inspectionHintID = "pistol_bullet_logo",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "logo_other_info",
                speaker = Speaker.Specialist,
                text = "Hm, without knowing the exact logo I can't tell you much. Get a closer look and call back if you can make it out.",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "engrave_ask",
                speaker = Speaker.Specialist,
                text = "Star-shaped?",
                nextNodeID = "engrave_choice"
            },
            new DialogueNode
            {
                nodeID = "engrave_choice",
                speaker = Speaker.Player,
                choices = new DialogueChoice[]
                {
                    new DialogueChoice { choiceText = "Yeah, it's a star.", nextNodeID = "engrave_star_info" },
                    new DialogueChoice { choiceText = "No, different shape.", nextNodeID = "engrave_other_info" }
                }
            },
            new DialogueNode
            {
                nodeID = "engrave_star_info",
                speaker = Speaker.Specialist,
                text = "Then grip color should be black, no exceptions on that combo. Serial number should start with a 7.",
                inspectionHintID = "pistol_star_engraving",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "engrave_other_info",
                speaker = Speaker.Specialist,
                text = "Not something I can cross-reference off the top of my head. Star engraving's the one with known specs.",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "chrome_info",
                speaker = Speaker.Specialist,
                text = "If it's genuinely chrome or nickel, manufacture year should be after 1990. And the serial shouldn't contain a zero anywhere.",
                inspectionHintID = "pistol_chrome_finish",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "wood_info",
                speaker = Speaker.Specialist,
                text = "Trigger guard should be rounded, not squared, to match that grip style. Serial number needs to be under 500.",
                inspectionHintID = "pistol_wood_grip",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "end",
                speaker = Speaker.Specialist,
                text = "Alright. Go through it careful before you hand over any cash. Good luck."
            }
        };

        SaveAsset(data, "Pistol_EugeneStoner");
    }

    // ---------------------------------------------------------
    // RIFLE — Mikhail Kalashnikov
    // ---------------------------------------------------------
    private static void GenerateRifle()
    {
        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.specialistName = "Mikhail Kalashnikov";
        data.callFee = 45;
        data.startNodeID = "start";

        data.nodes = new DialogueNode[]
        {
            new DialogueNode
            {
                nodeID = "start",
                speaker = Speaker.Player,
                text = "Got a rifle here, guy's asking $900.",
                nextNodeID = "greet"
            },
            new DialogueNode
            {
                nodeID = "greet",
                speaker = Speaker.Specialist,
                text = "Let's see what's on it — tell me what you notice.",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "hub",
                speaker = Speaker.Player,
                choices = new DialogueChoice[]
                {
                    new DialogueChoice { choiceText = "Star stamp on the receiver.", nextNodeID = "star_info" },
                    new DialogueChoice { choiceText = "There's a bayonet lug under the barrel.", nextNodeID = "bayonet_info" },
                    new DialogueChoice { choiceText = "Stock is black.", nextNodeID = "black_stock_info" },
                    new DialogueChoice { choiceText = "It's got a scope mounted.", nextNodeID = "scope_info" },
                    new DialogueChoice { choiceText = "No, that's all.", nextNodeID = "end" }
                }
            },
            new DialogueNode
            {
                nodeID = "star_info",
                speaker = Speaker.Specialist,
                text = "If that stamp's genuine, the stock should be wood. Serial number should fall between 200 and 650.",
                inspectionHintID = "rifle_star_stamp",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "bayonet_info",
                speaker = Speaker.Specialist,
                text = "Check the barrel finish — should be matte, not glossy. Serial number should start with a 4.",
                inspectionHintID = "rifle_bayonet_lug",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "black_stock_info",
                speaker = Speaker.Specialist,
                text = "If it's genuinely black, manufacture year should be after 1985. Serial number shouldn't contain a 9 anywhere.",
                inspectionHintID = "rifle_black_stock",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "scope_info",
                speaker = Speaker.Specialist,
                text = "Trigger guard should be metal on that setup. Serial number should be under 800.",
                inspectionHintID = "rifle_scope",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "end",
                speaker = Speaker.Specialist,
                text = "Alright, look it over careful. Good luck."
            }
        };

        SaveAsset(data, "Rifle_MikhailKalashnikov");
    }

    // ---------------------------------------------------------
    // SHOTGUN — John Browning
    // ---------------------------------------------------------
    private static void GenerateShotgun()
    {
        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.specialistName = "John Browning";
        data.callFee = 45;
        data.startNodeID = "start";

        data.nodes = new DialogueNode[]
        {
            new DialogueNode
            {
                nodeID = "start",
                speaker = Speaker.Player,
                text = "Got a shotgun here, guy's asking $700.",
                nextNodeID = "greet"
            },
            new DialogueNode
            {
                nodeID = "greet",
                speaker = Speaker.Specialist,
                text = "Alright, tell me what stands out on it.",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "hub",
                speaker = Speaker.Player,
                choices = new DialogueChoice[]
                {
                    new DialogueChoice { choiceText = "There's an engraved stamp on the receiver.", nextNodeID = "engrave_info" },
                    new DialogueChoice { choiceText = "It's got a double trigger.", nextNodeID = "double_trigger_info" },
                    new DialogueChoice { choiceText = "Handle's black.", nextNodeID = "black_handle_info" },
                    new DialogueChoice { choiceText = "There's a vent rib on top of the barrel.", nextNodeID = "vent_rib_info" },
                    new DialogueChoice { choiceText = "No, that's all.", nextNodeID = "end" }
                }
            },
            new DialogueNode
            {
                nodeID = "engrave_info",
                speaker = Speaker.Specialist,
                text = "If that engraving's legit, the stock should be wood. Serial number should fall between 150 and 500.",
                inspectionHintID = "shotgun_engraved_receiver",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "double_trigger_info",
                speaker = Speaker.Specialist,
                text = "Then the barrel should be side-by-side, not stacked. Serial number should start with a 2.",
                inspectionHintID = "shotgun_double_trigger",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "black_handle_info",
                speaker = Speaker.Specialist,
                text = "If it's genuinely black, manufacture year should be before 1960. Serial number shouldn't contain a 5 anywhere.",
                inspectionHintID = "shotgun_black_handle",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "vent_rib_info",
                speaker = Speaker.Specialist,
                text = "Stock should be green on that configuration. Serial number needs to be under 700.",
                inspectionHintID = "shotgun_vent_rib",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "end",
                speaker = Speaker.Specialist,
                text = "Alright, check it careful. Good luck."
            }
        };

        SaveAsset(data, "Shotgun_JohnBrowning");
    }

    // ---------------------------------------------------------
    // REVOLVER — Samuel Colt
    // ---------------------------------------------------------
    private static void GenerateRevolver()
    {
        var data = ScriptableObject.CreateInstance<DialogueData>();
        data.specialistName = "Samuel Colt";
        data.callFee = 40;
        data.startNodeID = "start";

        data.nodes = new DialogueNode[]
        {
            new DialogueNode
            {
                nodeID = "start",
                speaker = Speaker.Player,
                text = "Got a revolver here, guy's asking $550.",
                nextNodeID = "greet"
            },
            new DialogueNode
            {
                nodeID = "greet",
                speaker = Speaker.Specialist,
                text = "Let's hear it — what's it got on it?",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "hub",
                speaker = Speaker.Player,
                choices = new DialogueChoice[]
                {
                    new DialogueChoice { choiceText = "Eagle stamp on the frame.", nextNodeID = "eagle_info" },
                    new DialogueChoice { choiceText = "Cylinder looks fluted.", nextNodeID = "fluted_info" },
                    new DialogueChoice { choiceText = "Finish is nickel.", nextNodeID = "nickel_info" },
                    new DialogueChoice { choiceText = "Grips are ivory.", nextNodeID = "ivory_info" },
                    new DialogueChoice { choiceText = "No, that's all.", nextNodeID = "end" }
                }
            },
            new DialogueNode
            {
                nodeID = "eagle_info",
                speaker = Speaker.Specialist,
                text = "If that's legit, the grip should be pearl. Serial number should fall between 100 and 450.",
                inspectionHintID = "revolver_eagle_stamp",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "fluted_info",
                speaker = Speaker.Specialist,
                text = "Then the striker should be facing up at rest. Serial number should start with a 3.",
                inspectionHintID = "revolver_fluted_cylinder",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "nickel_info",
                speaker = Speaker.Specialist,
                text = "If it's genuinely nickel, manufacture year should be after 1970. Serial number shouldn't contain an 8 anywhere.",
                inspectionHintID = "revolver_nickel_finish",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "ivory_info",
                speaker = Speaker.Specialist,
                text = "Should have a bullet stamp on it somewhere. Serial number needs to be under 600.",
                inspectionHintID = "revolver_ivory_grips",
                nextNodeID = "hub"
            },
            new DialogueNode
            {
                nodeID = "end",
                speaker = Speaker.Specialist,
                text = "Alright, look it over careful. Good luck."
            }
        };

        SaveAsset(data, "Revolver_SamuelColt");
    }
}
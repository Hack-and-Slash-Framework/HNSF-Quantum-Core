using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace HnSF
{
    public abstract partial class BaseCommandListEntry : IContentDefinition
    {
        public string commandName;
        public string command;
        public bool requiresLockon;
        [TextArea]
        public string description;
        public string subtext;
        public string meterRequirement;
        
        public virtual BaseCommandListEntry[] ChildCommands { get; }
        
        public abstract Sprite GetImage();
        public abstract VideoClip GetVideo();

        public virtual string BuildCommandVisualText()
        {
            string assetName = "ButtonIcons";
            
            string r = "";

            for (var index = 0; index < command.ToLower().Length; index++)
            {
                var c = command.ToLower()[index];
                switch (c)
                {
                    case '5':
                        break;
                    case '8':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_UpArrow\">";
                        break;
                    case '2':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_DownArrow\">";
                        break;
                    case '4':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_LeftArrow\">";
                        break;
                    case '6':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_RightArrow\">";
                        break;
                    case 'l':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Light\">";
                        break;
                    case 'h':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Heavy\">";
                        break;
                    case 'u':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Unique\">";
                        break;
                    case 'd':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Dash\">";
                        break;
                    case 't':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Target\">";
                        break;
                    case 'j':
                        r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Jump\">";
                        break;
                    case 'a':
                        if (index < command.ToLower().Length - 1)
                        {
                            var nextChar = command.ToLower()[index + 1];
                            switch (nextChar)
                            {
                                case '1':
                                    r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Ability1\">";
                                    break;
                                case '2':
                                    r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Ability2\">";
                                    break;
                                case '3':
                                    r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Ability3\">";
                                    break;
                                case '4':
                                    r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Ability4\">";
                                    break;
                                case 'x':
                                    r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_Ability\">";
                                    break;
                                default:
                                    r += $"<sprite=\"{assetName}\" name=\"ButtonIcons_AbilityX\">";
                                    break;
                            }
                            index++;
                        }
                        break;
                    default:
                        r += c;
                        break;
                }
            }

            return r;
        }
    }
}
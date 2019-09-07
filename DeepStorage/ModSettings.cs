using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using UnityEngine;

namespace LWM.DeepStorage
{
    public class Settings : ModSettings {
        public static bool storingTakesTime=true;
        public static float storingGlobalScale=1f;
        public static bool storingTimeConsidersStackSize=true;
        public static StoragePriority defaultStoragePriority=StoragePriority.Important;

        // Architect Menu:
        // The defName for the DesignationCategoryDef the mod items are in by default:
        //TODO: make this a tutorial, provide link.
        // To use this code in another mod, change this const string, and then add to the
        // file ModName/Languages/English(etc)/Keyed/settings(or whatever).xml:
        // <[const string]_ArchitectMenuSettings>Location on Architect Menu:</...>
        // Copy and paste the rest of anything that says "Architect Menu"
        // Change the list of new mod items in the final place "Architect Menu" tells you to
        private const string architectMenuDefaultDesigCatDef="LWM_DS_Storage";
        private static string architectMenuDesigCatDef=architectMenuDefaultDesigCatDef;
        private static bool architectMenuAlwaysShowCategory=false;
        private static bool architectMenuMoveALLStorageItems=true;
        //   For later use if def is removed from menu...so we can put it back:
        private static DesignationCategoryDef architectMenuActualDef=null;
        private static bool architectMenuAlwaysShowTmp=false;
        private static bool architectMenuMoveALLTmp=true;


//        public static DesignationCategoryDef architectLWM_DS_Storage_DesignationCatDef=null; // keep track of this as it may be removed from DefDatabase
//        public static DesignationCategoryDef architectCurrentDesignationCatDef=null;

        private static List<ThingDef> allDeepStorageUnits=null;

        public static bool intelligenceWasChanged=false; // for switching away from HugsLib settings

//        private static float scrollViewHeight=100f;
        public static void DoSettingsWindowContents(Rect inRect) {
//            Setup();
            Listing_Standard l = new Listing_Standard(GameFont.Medium); // my tiny high-resolution monitor :p
            l.Begin(inRect);


            l.GapLine();  // Intelligence to haul to
            string [] intLabels={
                "LWM_DS_Int_Animal".Translate(),
                "LWM_DS_Int_ToolUser".Translate(),
                "LWM_DS_Int_Humanlike".Translate(),
            };
            // This setting was changed by HugsLib setting.  Am phasing out now:
            if (l.EnumRadioButton<Intelligence>(ref Patch_IsGoodStoreCell.NecessaryIntelligenceToUseDeepStorage, "LWM_DS_IntTitle".Translate(),
                                                "LWM_DS_IntDesc".Translate(), false, intLabels)) {
                intelligenceWasChanged=true;
            }

            
            l.GapLine();  //Storing Delay Settings
            l.Label("LWMDSstoringDelaySettings".Translate());
            l.CheckboxLabeled("LWMDSstoringTakesTimeLabel".Translate(),
                                             ref storingTakesTime, "LWMDSstoringTakesTimeDesc".Translate());
            l.Label("LWMDSstoringGlobalScale".Translate((storingGlobalScale*100f).ToString("0.")));
            storingGlobalScale=l.Slider(storingGlobalScale, 0f, 2f);
            l.CheckboxLabeled("LWMDSstoringTimeConsidersStackSize".Translate(),
                              ref storingTimeConsidersStackSize, "LWMDSstoringTimeConsidersStackSizeDesc".Translate());
            // Reset storing delay settings to defaults
            if (l.ButtonText("LWMDSstoringDelaySettings".Translate()+": "+"ResetBinding".Translate()/*Reset to Default*/)) {
                storingTakesTime=true;
                storingGlobalScale=1f;
                storingTimeConsidersStackSize=true;
            }
            l.GapLine(); // default Storing Priority
            if ( l.ButtonTextLabeled("LWM_DS_defaultStoragePriority".Translate(),
                                     defaultStoragePriority.Label()) ) {
                List<FloatMenuOption> mlist = new List<FloatMenuOption>();
                foreach (StoragePriority p in Enum.GetValues(typeof(StoragePriority))) {
                    mlist.Add(new FloatMenuOption(p.Label(), delegate() {
                                defaultStoragePriority=p;
                                foreach (ThingDef d in allDeepStorageUnits) {
                                    d.building.defaultStorageSettings.Priority=p;
                                }
                            }));
                }
                Find.WindowStack.Add(new FloatMenu(mlist));
            }


            // Architect Menu:
            l.GapLine();  //Architect Menu location
/*
//            string archLabel=
//            if (archLabel==n
//            l.Label("LWMDSarchitectMenuSettings".Translate());
            if (architectMenuDesigCatDef==architectMenuDefaultDesigCatDef) {
//                if (architectLWM_DS_Storage_DesignationCatDef==null) {
//                    Log.Error("LWM.DeepStorage: architectLWM_DS_Storage_DesignationCatDef was null; this should never happen.");
//                    tmp="ERROR";
//                } else {
//                    tmp=architectCurrentDesignationCatDef.LabelCap; // todo: (default)
//                }
                archLabel+=" ("+
            } else {
                var x=DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDesignationCatDefDefName, false);
                if (x==null) {
                    // TODO
                }
                tmp=x.LabelCap; // todo: (<menuname>)
            }*/
            if ( l.ButtonTextLabeled((architectMenuDefaultDesigCatDef+"_ArchitectMenuSettings").Translate(), // Label
                                     // value of dropdown button:
                                     DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDesigCatDef)?.LabelCap
                                     ?? "--ERROR--") ) { // error display text
//                                     , DefDatabase<DesigarchitectMenuDesigCatDef) ) {
                // Float menu for architect Menu choice:
                List<FloatMenuOption> alist = new List<FloatMenuOption>();
                var arl=DefDatabase<DesignationCategoryDef>.AllDefsListForReading; //all reading list
                //oops:
//                alist.Add(new FloatMenuOption(DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDefaultDesigCatDef).LabelCap
                alist.Add(new FloatMenuOption(architectMenuActualDef.LabelCap +" ("+"default".Translate()+")",
                                              delegate () {
                                                  Utils.Mess(Utils.DBF.Settings, "Architect Menu placement set to default Storage");
                                                  ArchitectMenu_ChangeLocation(architectMenuDefaultDesigCatDef);
//                                                  architectCurrentDesignationCatDef=architectLWM_DS_Storage_DesignationCatDef;
//                                                  architectMenuDesignationCatDefDefName="LWM_DS_Storage";
//
//                                                  SettingsChanged();
                                              }, MenuOptionPriority.Default, null, null, 0f, null, null));
                // Architect Menu:  You may remove the "Furniture" references here if you wish
                alist.Add(new FloatMenuOption(DefDatabase<DesignationCategoryDef>.GetNamed("Furniture").LabelCap,
                                              delegate () {
                                                  Utils.Mess(Utils.DBF.Settings, "Architect Menu placement set to Furniture.");
                                                  ArchitectMenu_ChangeLocation("Furniture");
                                              }, MenuOptionPriority.Default,null,null,0f,null,null));
                foreach (var adcd in arl) { //architect designation cat def
                    if (adcd.defName!=architectMenuDefaultDesigCatDef && adcd.defName!="Furniture")
                        alist.Add(new FloatMenuOption(adcd.LabelCap,
                                                      delegate () {
                                                          Utils.Mess(Utils.DBF.Settings, "Architect Menu placement set to "+adcd);
                                                          ArchitectMenu_ChangeLocation(adcd.defName);
                                                      }, MenuOptionPriority.Default,null,null,0f,null,null));
                }
                Find.WindowStack.Add(new FloatMenu(alist));
            }
            l.CheckboxLabeled((architectMenuDefaultDesigCatDef+"_ArchitectMenuAlwaysShowCategory").Translate(),
                              ref architectMenuAlwaysShowCategory,
                              (architectMenuDefaultDesigCatDef+"_ArchitectMenuAlwaysShowDesc").Translate());
            // Do we always display?  If so, display:
            if (architectMenuAlwaysShowCategory != architectMenuAlwaysShowTmp) {
                if (architectMenuAlwaysShowCategory) {
                    ArchitectMenu_Show();
                } else if (architectMenuDesigCatDef != architectMenuDefaultDesigCatDef) {
                    ArchitectMenu_Hide();
                }
                architectMenuAlwaysShowTmp=architectMenuAlwaysShowCategory;
            }
            l.CheckboxLabeled((architectMenuDefaultDesigCatDef+"_ArchitectMenuMoveALL").Translate(),
                              ref architectMenuMoveALLStorageItems,
                              (architectMenuDefaultDesigCatDef+"_ArchitectMenuMoveALLDesc").Translate());
            if (architectMenuMoveALLStorageItems != architectMenuMoveALLTmp) {
                //  If turning off "all things in Storage", make sure to
                //    dump all the items into Furniture, to make sure they
                //    can at least be found somewhere.
                string ctmp=architectMenuDesigCatDef;
                if (architectMenuMoveALLStorageItems==false) {
                    architectMenuMoveALLStorageItems=true;
                    ArchitectMenu_ChangeLocation("Furniture");
                    architectMenuMoveALLStorageItems=false;
                }
                ArchitectMenu_ChangeLocation(ctmp);
                architectMenuMoveALLTmp=architectMenuMoveALLStorageItems;
            }
            // finished drawing settings for Architect Menu

            l.End();
        }

        public static void DefsLoaded() {
            // Todo? If settings are different from defaults, then:
            Setup();
            // Architect Menu:
            if (architectMenuDesigCatDef != architectMenuDefaultDesigCatDef ||
                architectMenuMoveALLStorageItems) // in which case, we need to redo menu anyway
            {
                ArchitectMenu_ChangeLocation(architectMenuDesigCatDef, true);
            }
        }

        // Architect Menu:
        public static void ArchitectMenu_ChangeLocation(string newDefName, bool loadingOnStartup=false) {
//            Utils.Warn(Utils.DBF.Settings, "SettingsChanged()");
            DesignationCategoryDef prevDesignationCatDef;
            if (loadingOnStartup) prevDesignationCatDef=DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDefaultDesigCatDef);
            else prevDesignationCatDef=DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDesigCatDef, false);
            // If switching to default, put default into def database.
            if (newDefName == architectMenuDefaultDesigCatDef) {
                ArchitectMenu_Show();
            }
            DesignationCategoryDef newDesignationCatDef=DefDatabase<DesignationCategoryDef>.GetNamed(newDefName);
            if (newDesignationCatDef == null) {
                Log.Error("Failed to change Architect Menu settings!");
                return;
            }
            // Architect Menu: Specify all your buildings/etc:
            //   var allMyBuildings=DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x=>x.HasComp(etc)));
            List<ThingDef> itemsToMove=allDeepStorageUnits;
            // We can move ALL the storage buildings!  If the player wants.  I do.
            if (architectMenuMoveALLStorageItems) {
//                Log.Error("Trying to mvoe everythign:");
                var desigProduction=DefDatabase<DesignationCategoryDef>.GetNamed("Production");
                itemsToMove=DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x=>(x!=null && (x.thingClass==typeof(Building_Storage) ||
                                                                                     x.thingClass.IsSubclassOf(typeof(Building_Storage)))
                                                                                    && x.designationCategory!=desigProduction
                                                                                    ));
                // testing:
//                itemsToMove.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x=>x.defName.Contains("MURWallLight")));
            }
            var _resolvedDesignatorsField = typeof(DesignationCategoryDef)
                .GetField("resolvedDesignators", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var d in itemsToMove) {
                if (d.designationCategory==null) continue; // very very possible
//                Log.Error("Moving item "+d.defName+" (category: "+(d.designationCategory!=null?d.designationCategory.ToString():"NONE"));
                List<Designator> resolvedDesignators= (List<Designator>)_resolvedDesignatorsField.GetValue(d.designationCategory);
                if (d.designatorDropdown == null) {
//                    Log.Message("No dropdown");
                    // easy case:
//                    Log.Message("  Removed this many entries in "+d.designationCategory+": "+
                    resolvedDesignators.RemoveAll(x=>((x is Designator_Build) &&
                                                      ((Designator_Build)x).PlacingDef==d));
//                        );
                    // Now do new:
                    resolvedDesignators=(List<Designator>)_resolvedDesignatorsField.GetValue(newDesignationCatDef);
                    // To make sure there are no duplicates:
                    resolvedDesignators.RemoveAll(x=>((x is Designator_Build) &&
                                                      ((Designator_Build)x).PlacingDef==d));
                    resolvedDesignators.Add(new Designator_Build(d));
                } else {
//                    Log.Warning("LWM.DeepStorage: ThingDef "+d.defName+" has a dropdown Designator.");
                    // Hard case: Designator_Dropdowns!
                    Designator_Dropdown dd=(Designator_Dropdown)resolvedDesignators.Find(x=>(x is Designator_Dropdown) &&
                                                                    ((Designator_Dropdown)x).Elements
                                                                    .Find(y=>(y is Designator_Build) &&
                                                                          ((Designator_Build)y).PlacingDef==d)!=null);
                    if (dd != null) {
//                        Log.Message("Found dropdown designator for "+d.defName);
                        resolvedDesignators.Remove(dd);
                        // Switch to new category:
                        resolvedDesignators=(List<Designator>)_resolvedDesignatorsField.GetValue(newDesignationCatDef);
                        if (!resolvedDesignators.Contains(dd)) {
//                            Log.Message("  Adding to new category "+newDesignationCatDef);
                            resolvedDesignators.Add(dd);
                        }
//                    } else { //debug
//                        Log.Message("   ThingDef "+d.defName+" has designator_dropdown "+d.designatorDropdown.defName+
//                            ", but cannot find it in "+d.designationCategory+" - this is okay if something else added it.");
                    }
                }
                d.designationCategory=newDesignationCatDef;
            }
            // Flush designation category defs:.....dammit
//            foreach (var x in DefDatabase<DesignationCategoryDef>.AllDefs) {
//                x.ResolveReferences();
//            }
//            prevDesignationCatDef?.ResolveReferences();
//            newDesignationCatDef.ResolveReferences();
            //ArchitectMenu_ClearCache(); // we do this later one way or another
            
            // To remove the mod's DesignationCategoryDef from Architect menu:
            //   remove it from RimWorld.MainTabWindow_Architect's desPanelsCached.
            // To do that, we remove it from the DefDatabase and then rebuild the cache.
            //   Removing only the desPanelsCached entry does work: the entry is
            //   recreated when a new game is started.  So if the options are changed
            //   and then a new game started...the change gets lost.
            // So we have to update the DefsDatabase.
            // This is potentially difficult: the .index can get changed, and that
            //   can cause problems.  But nothing seems to use the .index for any
            //   DesignationCategoryDef except for the menu, so manually adjusting
            //   the DefsDatabase is safe enough:
            if (!architectMenuAlwaysShowCategory && newDefName != architectMenuDefaultDesigCatDef) {
                ArchitectMenu_Hide();
                // ArchitectMenu_ClearCache(); //hide flushes cache
//                    if (tmp.AllResolvedDesignators.Count <= tmp.specialDesignatorClasses.Count)
//                        isCategoryEmpty=false;
/*                    
//                    Log.Message("Removing old menu!");
                    // DefDatabase<DesignationCategoryDef>.Remove(tmp);
                    if (!tmp.AllResolvedDesignators.NullOrEmpty()) {
                        foreach (var d in tmp.AllResolvedDesignators) {
                            if (!tmp.specialDesignatorClasses.Contains(d)) {
                                isCategoryEmpty=false;
                                break;
                            }
                        }
                    }
                    */
//                    if (isCategoryEmpty)
            } else {
                // Simply flush cache:
                ArchitectMenu_ClearCache();

            }
            // Note that this is not perfect: if the default menu was already open, it will still be open (and
            //   empty) when the settings windows are closed.  Whatever.


            // Oh, and actually change the setting that's stored:
            architectMenuDesigCatDef=newDefName;

/*            List<ArchitectCategoryTab> archMenu=(List<ArchitectCategoryTab>)Harmony.AccessTools
                .Field(typeof(RimWorld.MainTabWindow_Architect), "desPanelsCached")
                .GetValue((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow);
            archMenu.RemoveAll(t=>t.def.defName==architectMenuDefaultDesigCatDef);

            archMenu.Add(new ArchitectCategoryTab(newDesignationCatDef));
            archMenu.Sort((a,b)=>a.def.order.CompareTo(b.def.order));
            archMenu.SortBy(a=>a.def.order, b=>b.def.order); // May need (type of var a)=>...

            */





            
/*            Harmony.AccessTools.Method(typeof(RimWorld.MainTabWindow_Architect), "CacheDesPanels")
                .Invoke(((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow), null);*/


/*
            
            if (architectMenuDesignationCatDefDefName=="LWM_DS_Storage") { // default
                if (DefDatabase<DesignationCategoryDef>.GetNamedSilentFail("LWM_DS_Storage") == null) {
                    Utils.Mess(Utils.DBF.Settings,"Adding 'Storage' to the architect menu.");
                    DefDatabase<DesignationCategoryDef>.Add(architectLWM_DS_Storage_DesignationCatDef);
                } else {
                    Utils.Mess(Utils.DBF.Settings, "No need to add 'Storage' to the architect menu.");
                }
                architectCurrentDesignationCatDef=architectLWM_DS_Storage_DesignationCatDef;
            } else {
                // remove our "Storage" from the architect menu:
                Utils.Mess(Utils.DBF.Settings,"Removing 'Storage' from the architect menu.");
                DefDatabase<DesignationCategoryDef>.AllDefsListForReading.Remove(architectLWM_DS_Storage_DesignationCatDef);
                if (DefDatabase<DesignationCategoryDef>.GetNamedSilentFail("LWM_DS_Storage") != null) {
                    Log.Error("Failed to remove LWM_DS_Storage :("+DefDatabase<DesignationCategoryDef>.GetNamedSilentFail("LWM_DS_Storage"));
                }

                architectCurrentDesignationCatDef=DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDesignationCatDefDefName);
            }
            prevDesignationCatDef?.ResolveReferences();
            architectCurrentDesignationCatDef.ResolveReferences();
            
            Harmony.AccessTools.Method(typeof(RimWorld.MainTabWindow_Architect), "CacheDesPanels")
                .Invoke((), null);
*/
            Utils.Warn(Utils.DBF.Settings, "Settings changed architect menu");
            
        }
        public static void ArchitectMenu_Hide() {
            DesignationCategoryDef tmp;
            if ((tmp=DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDefaultDesigCatDef, false))!=null
                && !architectMenuAlwaysShowCategory) {
                // DefDatabase<DesignationCategoryDef>.Remove(tmp);
                typeof(DefDatabase<>).MakeGenericType(new Type[] {typeof(DesignationCategoryDef)})
                    .GetMethod("Remove", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                    .Invoke (null, new object [] { tmp });
                // No need to SetIndices() or anything: .index are not used for DesignationCategoryDef(s).  I hope.
            }
            ArchitectMenu_ClearCache();
        }

        public static void ArchitectMenu_Show() {
            if (DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDefaultDesigCatDef, false)==null) {
                DefDatabase<DesignationCategoryDef>.Add(architectMenuActualDef);
            }
            ArchitectMenu_ClearCache();
        }

        public static void ArchitectMenu_ClearCache() {
            // Clear the architect menu cache:
            //   Run the main Architect.TabWindow.CacheDesPanels()
            typeof(RimWorld.MainTabWindow_Architect).GetMethod("CacheDesPanels", System.Reflection.BindingFlags.NonPublic |
                                                                     System.Reflection.BindingFlags.Instance)
                .Invoke(((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow), null);
        }

        
        // Setup stuff that needs to be run before settings can be used.
        //   I don't risk using a static constructor because I must make sure defs have been finished loading.
        //     (testing shows this is VERY correct!!)
        //   There's probably some rimworld annotation that I could use, but this works:
        private static void Setup() {
            if (architectMenuActualDef==null) {
                architectMenuActualDef=DefDatabase<DesignationCategoryDef>.GetNamed(architectMenuDefaultDesigCatDef);
            }
            if (allDeepStorageUnits.NullOrEmpty()) {
                allDeepStorageUnits=DefDatabase<ThingDef>.AllDefsListForReading.FindAll(x=>x.HasComp(typeof(CompDeepStorage)));
                Utils.Mess(Utils.DBF.Settings, "  allDeepStorageUnits initialized: "+allDeepStorageUnits.Count+" units");
            }
            /*           if (architectLWM_DS_Storage_DesignationCatDef==null) {
                architectLWM_DS_Storage_DesignationCatDef=DefDatabase<DesignationCategoryDef>.GetNamed("LWM_DS_Storage");
                Utils.Mess(Utils.DBF.Settings, "  Designation Category Def loaded: "+architectLWM_DS_Storage_DesignationCatDef);
            }*/
        }

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref storingTakesTime, "storing_takes_time", true);
            Scribe_Values.Look(ref storingGlobalScale, "storing_global_scale", 1f);
            Scribe_Values.Look(ref Patch_IsGoodStoreCell.NecessaryIntelligenceToUseDeepStorage, "int_to_use_DS", Intelligence.Humanlike);
            Scribe_Values.Look(ref intelligenceWasChanged, "int_was_changed", false);
            Scribe_Values.Look(ref defaultStoragePriority, "default_s_priority", StoragePriority.Important);
            // Architect Menu:
            Scribe_Values.Look(ref architectMenuDesigCatDef, "architect_desig", architectMenuDefaultDesigCatDef);
            Scribe_Values.Look(ref architectMenuAlwaysShowCategory, "architect_show", false);
            Scribe_Values.Look(ref architectMenuMoveALLStorageItems, "architect_moveall", true);
        }
    }

    static class DisplayHelperFunctions {
        // Helper function to create EnumRadioButton for Enums in settings
        public static bool EnumRadioButton<T>(this Listing_Standard ls, ref T val, string label, string tooltip="",
                                              bool showEnumValues=true, string[] buttonLabels=null) {
            if ((val as Enum)==null) {
                Log.Error("LWM.DisplayHelperFunction: EnumRadioButton passed non-enum value");
                return false;
            }
            bool result=false;
            if (tooltip=="")
                ls.Label(label);
            else
                ls.Label(label,-1,tooltip);
            var enumValues = Enum.GetValues(val.GetType());
            int i=0;
            foreach (T x in enumValues) {
                string optionLabel;
                if (showEnumValues || buttonLabels==null) {
                    optionLabel=x.ToString();
                    if (buttonLabels != null) {
                        optionLabel+=": "+buttonLabels[i];
                    }
                } else {
                    optionLabel=buttonLabels[i]; // got a better way?
                }
                if (ls.RadioButton(optionLabel, (val.ToString()==x.ToString()))) {
                    val=x; // I swear....ToString() was the only thing that worked.
                    result=true;
                } // nice try, C#, nice try.
                // ((val as Enum)==(x as Enum)) // nope
                // (System.Convert.ChangeType(val, Enum.GetUnderlyingType(val.GetType()))==
                //  System.Convert.ChangeType(  x, Enum.GetUnderlyingType(  x.GetType()))) // nope
                // (x.ToString()==val.ToString())// YES!
                i++;
            }
            return result;
        }
    } // end DisplayHelperFunctions


}
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vintagestory.API.Common;
using BalancedThirst.Config;

namespace BalancedThirst.HoDCompat
{
    public static class ItemHydrationConfigLoader
    {
        public static List<JObject> LoadHydrationPatches(ICoreAPI api)
        {
            List<JObject> allPatches = new List<JObject>();
            string configFolder = ModConfig.GetConfigPath(api);
            List<string> configFiles = Directory.GetFiles(configFolder, "*AddItemHydration*.json").ToList();
            string defaultConfigPath = Path.Combine(configFolder, "HoD.AddItemHydration.json");
            if (!File.Exists(defaultConfigPath))
            {
                GenerateDefaultHydrationConfig(api);
            }
            string btConfigPath = Path.Combine(configFolder, "BT.AddItemHydration.json");
            if (!File.Exists(btConfigPath))
            {
                GenerateBTHydrationConfig(api);
            }
            configFiles.Insert(0, btConfigPath);
            var sortedPatches = new SortedDictionary<int, List<JObject>>();

            foreach (string file in configFiles)
            {
                    string json = File.ReadAllText(file);
                    JObject parsedFile = JObject.Parse(json);
                    int priority = parsedFile["priority"]?.Value<int>() ?? 5;

                    if (!sortedPatches.ContainsKey(priority))
                    {
                        sortedPatches[priority] = new List<JObject>();
                    }

                    var patches = parsedFile["patches"]?.ToObject<List<JObject>>();
                    if (patches != null) sortedPatches[priority].AddRange(patches);
            }
            Dictionary<string, JObject> mergedPatches = new Dictionary<string, JObject>();

            foreach (var priorityLevel in sortedPatches.Keys.OrderByDescending(k => k))
            {
                foreach (var patch in sortedPatches[priorityLevel])
                {
                    string itemname = patch["itemname"]?.ToString();
                    if (itemname != null) mergedPatches[itemname] = patch;
                }
            }

            return mergedPatches.Values.ToList();
        }

        public static void GenerateBTHydrationConfig(ICoreAPI api)
        {
            string configPath = Path.Combine(ModConfig.GetConfigPath(api), "Bt.AddItemHydration.json");
            if (!File.Exists(configPath))
            {
                var defaultConfig = new JObject
                {
                    ["priority"] = 2,
                    ["patches"] = new JArray
                    {
                        new JObject
                        {
                            ["itemname"] = "balancedthirst:waterportion-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["balancedthirst:waterportion-pure"] = 1000,
                                ["balancedthirst:waterportion-boiled"] = 800,
                                ["balancedthirst:waterportion-stagnant"] = 200,
                                ["*"] = 600
                            },
                            ["IsLiquid"] = true
                        },
                    }
                };
                    File.WriteAllText(configPath, defaultConfig.ToString());
            }
        }
        
        public static void GenerateDefaultHydrationConfig(ICoreAPI api)
        {
            string configPath = Path.Combine(ModConfig.GetConfigPath(api), "HoD.AddHydration.json");
            if (!File.Exists(configPath))
            {
                var defaultConfig = new JObject
                {
                    ["priority"] = 5,
                    ["patches"] = new JArray
                    {
                        new JObject
                        {
                            ["itemname"] = "game:juiceportion-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:juiceportion-cranberry"] = 600,
                                ["game:juiceportion-blueberry"] = 700,
                                ["game:juiceportion-pinkapple"] = 750,
                                ["game:juiceportion-lychee"] = 850,
                                ["game:juiceportion-redcurrant"] = 800,
                                ["game:juiceportion-breadfruit"] = 500,
                                ["game:juiceportion-pineapple"] = 950,
                                ["game:juiceportion-blackcurrant"] = 800,
                                ["game:juiceportion-saguaro"] = 600,
                                ["game:juiceportion-whitecurrant"] = 800,
                                ["game:juiceportion-redapple"] = 900,
                                ["game:juiceportion-yellowapple"] = 900,
                                ["game:juiceportion-cherry"] = 800,
                                ["game:juiceportion-peach"] = 950,
                                ["game:juiceportion-pear"] = 950,
                                ["game:juiceportion-orange"] = 1000,
                                ["game:juiceportion-mango"] = 950,
                                ["game:juiceportion-pomegranate"] = 850,
                                ["*"] = 750
                            },
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:fruit-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:fruit-cranberry"] = 10,
                                ["game:fruit-blueberry"] = 7,
                                ["game:fruit-pinkapple"] = 12,
                                ["game:fruit-lychee"] = 20,
                                ["game:fruit-redcurrant"] = 15,
                                ["game:fruit-breadfruit"] = 5,
                                ["game:fruit-pineapple"] = 20,
                                ["game:fruit-blackcurrant"] = 15,
                                ["game:fruit-saguaro"] = 10,
                                ["game:fruit-whitecurrant"] = 15,
                                ["game:fruit-redapple"] = 18,
                                ["game:fruit-yellowapple"] = 18,
                                ["game:fruit-cherry"] = 15,
                                ["game:fruit-peach"] = 25,
                                ["game:fruit-pear"] = 20,
                                ["game:fruit-orange"] = 30,
                                ["game:fruit-mango"] = 25,
                                ["game:fruit-pomegranate"] = 20,
                                ["*"] = 8
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:bambooshoot",
                            ["hydration"] = 15,
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:bread-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:bread-spelt"] = -5,
                                ["game:bread-rye"] = -4,
                                ["game:bread-flax"] = -3,
                                ["game:bread-rice"] = -6,
                                ["game:bread-cassava"] = -5,
                                ["game:bread-amaranth"] = -4,
                                ["game:bread-sunflower"] = -3,
                                ["game:bread-spelt-partbaked"] = -5,
                                ["game:bread-rye-partbaked"] = -4,
                                ["game:bread-flax-partbaked"] = -3,
                                ["game:bread-rice-partbaked"] = -6,
                                ["game:bread-cassava-partbaked"] = -5,
                                ["game:bread-amaranth-partbaked"] = -4,
                                ["game:bread-sunflower-partbaked"] = -3,
                                ["game:bread-spelt-perfect"] = -5,
                                ["game:bread-rye-perfect"] = -4,
                                ["game:bread-flax-perfect"] = -3,
                                ["game:bread-rice-perfect"] = -6,
                                ["game:bread-cassava-perfect"] = -5,
                                ["game:bread-amaranth-perfect"] = -4,
                                ["game:bread-sunflower-perfect"] = -3,
                                ["game:bread-spelt-charred"] = -10,
                                ["game:bread-rye-charred"] = -9,
                                ["game:bread-flax-charred"] = -8,
                                ["game:bread-rice-charred"] = -11,
                                ["game:bread-cassava-charred"] = -10,
                                ["game:bread-amaranth-charred"] = -9,
                                ["game:bread-sunflower-charred"] = -8,
                                ["*"] = -5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:bushmeat-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:bushmeat-cooked"] = -5,
                                ["game:bushmeat-cured"] = -10,
                                ["*"] = -5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:butter",
                            ["hydration"] = -5,
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:cheese-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:cheese-blue-1slice"] = -4,
                                ["game:cheese-cheddar-1slice"] = -3
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:dough-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:dough-spelt"] = 5,
                                ["game:dough-rye"] = 5,
                                ["game:dough-flax"] = 5,
                                ["game:dough-rice"] = 5,
                                ["game:dough-cassava"] = 5,
                                ["game:dough-amaranth"] = 5,
                                ["game:dough-sunflower"] = 5,
                                ["*"] = 5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:fish-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:fish-raw"] = 5,
                                ["game:fish-cooked"] = -2,
                                ["game:fish-cured"] = -10,
                                ["game:fish-smoked"] = -8,
                                ["game:fish-cured-smoked"] = -12,
                                ["*"] = -5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:grain-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:grain-spelt"] = -3,
                                ["game:grain-rice"] = -3,
                                ["game:grain-flax"] = -3,
                                ["game:grain-rye"] = -3,
                                ["game:grain-amaranth"] = -3,
                                ["game:grain-sunflower"] = -3,
                                ["*"] = -3
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:insect-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:insect-grub"] = 2,
                                ["game:insect-termite"] = 2,
                                ["game:insect-termite-stick"] = 2,
                                ["*"] = 2
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:legume-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:legume-peanut"] = -3,
                                ["*"] = -3
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:pemmican-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:pemmican-raw-basic"] = -5,
                                ["game:pemmican-raw-salted"] = -7,
                                ["game:pemmican-dried-basic"] = -10,
                                ["game:pemmican-dried-salted"] = -12,
                                ["*"] = -5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:pickledlegume-soybean",
                            ["hydration"] = 2,
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:pickledvegetable-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:pickledvegetable-carrot"] = 3,
                                ["game:pickledvegetable-cabbage"] = 3,
                                ["game:pickledvegetable-onion"] = 2,
                                ["game:pickledvegetable-turnip"] = 3,
                                ["game:pickledvegetable-parsnip"] = 3,
                                ["game:pickledvegetable-pumpkin"] = 4,
                                ["game:pickledvegetable-bellpepper"] = 4,
                                ["game:pickledvegetable-olive"] = 1,
                                ["*"] = 3
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:poultry-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:poultry-cooked"] = -5,
                                ["game:poultry-cured"] = -10,
                                ["*"] = -5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:redmeat-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:redmeat-cooked"] = -5,
                                ["game:redmeat-vintage"] = -8,
                                ["game:redmeat-cured"] = -10,
                                ["*"] = -5
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:vegetable-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:vegetable-carrot"] = 8,
                                ["game:vegetable-cabbage"] = 10,
                                ["game:vegetable-onion"] = 6,
                                ["game:vegetable-turnip"] = 8,
                                ["game:vegetable-parsnip"] = 7,
                                ["game:vegetable-cookedcattailroot"] = 5,
                                ["game:vegetable-pumpkin"] = 12,
                                ["game:vegetable-cassava"] = 6,
                                ["game:vegetable-cookedpapyrusroot"] = 5,
                                ["game:vegetable-bellpepper"] = 12,
                                ["game:vegetable-olive"] = 4,
                                ["*"] = 7
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:alcoholportion",
                            ["hydration"] = -20,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:boilingwaterportion",
                            ["hydration"] = 1500,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:ciderportion-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:ciderportion-cranberry"] = 300,
                                ["game:ciderportion-blueberry"] = 350,
                                ["game:ciderportion-pinkapple"] = 375,
                                ["game:ciderportion-lychee"] = 425,
                                ["game:ciderportion-redcurrant"] = 400,
                                ["game:ciderportion-breadfruit"] = 250,
                                ["game:ciderportion-pineapple"] = 475,
                                ["game:ciderportion-blackcurrant"] = 400,
                                ["game:ciderportion-saguaro"] = 300,
                                ["game:ciderportion-whitecurrant"] = 400,
                                ["game:ciderportion-redapple"] = 450,
                                ["game:ciderportion-yellowapple"] = 450,
                                ["game:ciderportion-cherry"] = 400,
                                ["game:ciderportion-peach"] = 475,
                                ["game:ciderportion-pear"] = 475,
                                ["game:ciderportion-orange"] = 500,
                                ["game:ciderportion-mango"] = 475,
                                ["game:ciderportion-pomegranate"] = 425,
                                ["game:ciderportion-apple"] = 450,
                                ["game:ciderportion-mead"] = 400,
                                ["game:ciderportion-spelt"] = 450, 
                                ["game:ciderportion-rice"] = 450, 
                                ["game:ciderportion-rye"] = 450, 
                                ["game:ciderportion-amaranth"] = 450, 
                                ["game:ciderportion-cassava"] = 450,
                                ["*"] = 375 
                            },
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:honeyportion",
                            ["hydration"] = 300,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:jamhoneyportion",
                            ["hydration"] = 350,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:saltwaterportion",
                            ["hydration"] = -600,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:vinegarportion",
                            ["hydration"] = 50,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:cottagecheeseportion",
                            ["hydration"] = 50,
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:milkportion",
                            ["hydration"] = 500,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:waterportion",
                            ["hydration"] = 600,
                            ["IsLiquid"] = true
                        },
                        new JObject
                        {
                            ["itemname"] = "game:mushroom-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:mushroom-flyagaric"] = -5,
                                ["game:mushroom-earthball"] = -6,
                                ["game:mushroom-deathcap"] = -15,
                                ["game:mushroom-elfinsaddle"] = -6,
                                ["game:mushroom-jackolantern"] = -5,
                                ["game:mushroom-devilbolete"] = -8,
                                ["game:mushroom-bitterbolete"] = -1,
                                ["game:mushroom-devilstooth"] = -2,
                                ["game:mushroom-golddropmilkcap"] = -2,
                                ["game:mushroom-beardedtooth"] = 1,
                                ["game:mushroom-whiteoyster"] = 1,
                                ["game:mushroom-pinkoyster"] = 1,
                                ["game:mushroom-dryadsaddle"] = 1,
                                ["game:mushroom-tinderhoof"] = 1,
                                ["game:mushroom-chickenofthewoods"] = 1,
                                ["game:mushroom-reishi"] = 1,
                                ["game:mushroom-funeralbell"] = -20,
                                ["game:mushroom-livermushroom"] = 1,
                                ["game:mushroom-pinkbonnet"] = -5,
                                ["game:mushroom-shiitake"] = 1,
                                ["game:mushroom-deerear"] = 1,
                                ["*"] = 0
                            },
                            ["IsLiquid"] = false
                        },
                        new JObject
                        {
                            ["itemname"] = "game:spiritportion-*",
                            ["hydrationByType"] = new JObject
                            {
                                ["game:spiritportion-cranberry"] = 160,
                                ["game:spiritportion-blueberry"] = 180,
                                ["game:spiritportion-pinkapple"] = 190,
                                ["game:spiritportion-lychee"] = 220,
                                ["game:spiritportion-redcurrant"] = 210,
                                ["game:spiritportion-breadfruit"] = 130,
                                ["game:spiritportion-pineapple"] = 250,
                                ["game:spiritportion-blackcurrant"] = 210,
                                ["game:spiritportion-saguaro"] = 160,
                                ["game:spiritportion-whitecurrant"] = 210,
                                ["game:spiritportion-redapple"] = 240,
                                ["game:spiritportion-yellowapple"] = 240,
                                ["game:spiritportion-cherry"] = 210,
                                ["game:spiritportion-peach"] = 250,
                                ["game:spiritportion-pear"] = 250,
                                ["game:spiritportion-orange"] = 270,
                                ["game:spiritportion-mango"] = 250,
                                ["game:spiritportion-pomegranate"] = 220,
                                ["game:spiritportion-apple"] = 240, 
                                ["game:spiritportion-mead"] = 200, 
                                ["game:spiritportion-spelt"] = 225,
                                ["game:spiritportion-rice"] = 225, 
                                ["game:spiritportion-rye"] = 225,
                                ["game:spiritportion-amaranth"] = 225,
                                ["game:spiritportion-cassava"] = 225, 
                                ["*"] = 190 
                            },
                            ["IsLiquid"] = true
                        }
                    }
                };
                    File.WriteAllText(configPath, defaultConfig.ToString());
            }
        }
    }
}
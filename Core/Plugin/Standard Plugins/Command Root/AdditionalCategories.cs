using Lomztein.AdvDiscordCommands.Framework.Categories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lomztein.Moduthulhu.Plugins.Standard

{
    public static class AdditionalCategories
    {
        public static readonly Category Management = new Category ("Management", "Commands for managing the bot and its features, such as configuration and modules.");

        public static readonly Category Voice = new Category ("Voice", "Commands revolving the interaction and expansion of Discords voice channels.");
    }
}

[assembly: CommandClass(typeof(KuroAcad.CmdShowPalette))]
namespace KuroAcad
{
    class CmdShowPalette
    {
        static CustomPaletteSet paletteSet;

        [CommandMethod("KUROSHOWPALETTE")]
        public static void ShowPalette()
        {
            if (paletteSet == null)
            {
                paletteSet = new CustomPaletteSet();
                paletteSet.Palette.HelloText = "Hello World!";
            }
            paletteSet.Visible = true;
        }
    }
}

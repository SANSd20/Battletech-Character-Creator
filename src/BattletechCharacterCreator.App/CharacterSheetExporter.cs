using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Resources;
using BattletechCharacterCreator.Core.Rules;

namespace BattletechCharacterCreator.App;

public static class CharacterSheetExporter
{
    private const double SheetWidth = 2479;
    private const double SheetHeight = 3508;
    private const double PageWidth = 612;
    private const double PageHeight = 792;
    private const double TextBaselineLift = 11;

    public static void Export(
        Character character,
        ResourceCatalog catalog,
        string path)
    {
        var sheetDirectory = Path.Combine(
            AppContext.BaseDirectory, "Assets", "Sheets");
        var front = LoadImage(Path.Combine(
            sheetDirectory, "CharacterRecordSheet.png"));
        var reverse = LoadImage(Path.Combine(
            sheetDirectory, "CharacterRecordSheetReverse.png"));
        var needsAdditional =
            character.Traits.Count > 8 || character.Skills.Count > 30;
        var pages = new List<SheetPage>
        {
            new(front, BuildFront(character, catalog)),
            new(reverse, BuildReverse(character))
        };
        if (needsAdditional)
        {
            pages.Add(new SheetPage(
                LoadImage(Path.Combine(
                    sheetDirectory, "CharacterRecordSheetAdditional.png")),
                BuildAdditional(character, catalog)));
        }

        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllBytes(path, BuildPdf(pages));
    }

    private static string BuildFront(
        Character character,
        ResourceCatalog catalog)
    {
        var summary = CharacterRules.Calculate(character);
        var canvas = new PdfCanvas();
        canvas.Text(300, 523, character.Name, 30);
        canvas.Text(340, 594, character.Height.ToString(), 30);
        canvas.Text(1000, 594, character.Weight.ToString(), 30);
        canvas.Text(1550, 594,
            Join(character.Affiliation, character.SubAffiliation), 20);
        canvas.Text(320, 670, character.HairColor, 30);
        canvas.Text(960, 670, character.EyeColor, 30);

        var attributeY = new[] { 948, 1000, 1053, 1105, 1158, 1211, 1263, 1316 };
        for (var index = 0;
             index < character.Attributes.Count && index < attributeY.Length;
             index++)
        {
            var attribute = character.Attributes[index];
            var value = CharacterRules.AttributeValue(attribute.Value);
            canvas.Text(490, attributeY[index], value.ToString(), 30);
            canvas.Text(745, attributeY[index],
                CharacterRules.LinkModifier(value).ToString("+0;-0;0"), 30);
            canvas.TextRight(
                1090, attributeY[index], attribute.Value.ToString(), 30);
        }

        canvas.Text(1535, 1243, summary.Walk.ToString(), 30);
        canvas.Text(1525, 1303, summary.Run.ToString(), 30);
        canvas.Text(1525, 1363, summary.Sprint.ToString(), 30);
        canvas.Text(2085, 1243, summary.Climb.ToString(), 30);
        canvas.Text(2085, 1303, summary.Crawl.ToString(), 30);
        canvas.Text(2085, 1363, summary.Swim.ToString(), 30);

        var traits = character.Traits.OrderBy(item => item.Name).Take(8).ToArray();
        for (var index = 0; index < traits.Length; index++)
        {
            var y = 1571 + index * 59;
            WriteTrait(canvas, traits[index], catalog, 150, 715, 825, 1112, y);
        }

        var skills = character.Skills.OrderBy(item => item.Name).Take(30).ToArray();
        for (var index = 0; index < skills.Length; index++)
        {
            var secondColumn = index >= 15;
            var row = secondColumn ? index - 15 : index;
            var y = 2261 + row * 74;
            WriteSkill(canvas, skills[index], catalog,
                secondColumn ? 1280 : 150,
                secondColumn ? 1848 : 718,
                secondColumn ? 1930 : 800,
                secondColumn ? 2302 : 1179,
                y,
                character.Traits);
        }

        canvas.Text(1490, 1790,
            CharacterRules.SkillLevel(
                Find(character.Skills, "Martial Arts"), character.Traits).ToString(), 30);
        canvas.Text(1540, 1790, "0", 30);
        canvas.Text(1630, 1790,
            Math.Max(1, (int)Math.Ceiling(
                CharacterRules.AttributeValue(
                    Find(character.Attributes, "STR")) / 4d)).ToString(), 30);

        var weapons = character.Weapons.Take(8).ToArray();
        for (var index = 0; index < weapons.Length; index++)
        {
            var weapon = weapons[index];
            var y = 1850 + index * 60;
            canvas.Text(1220, y, weapon.Name, 20);
            canvas.Text(1490, y,
                CharacterRules.SkillLevel(
                    Find(character.Skills, weapon.Skill), character.Traits).ToString(), 30);
            var damage = Split(weapon.Damage);
            canvas.Text(1540, y, damage.ElementAtOrDefault(0) ?? "", 30);
            canvas.Text(1630, y, damage.ElementAtOrDefault(1) ?? "", 30);
            var ranges = Split(weapon.Range);
            for (var range = 0; range < Math.Min(4, ranges.Length); range++)
            {
                canvas.Text(new[] { 1720, 1772, 1830, 1881 }[range],
                    y, ranges[range], 20);
            }
            canvas.Text(1975, y, weapon.Shots, 20);
            canvas.Text(2080, y, weapon.Notes, 20, 260);
        }

        canvas.Text(1670, 435,
            $"Char# {CharacterId(character):000000000}", 20);
        return canvas.ToString();
    }

    private static string BuildReverse(Character character)
    {
        var summary = CharacterRules.Calculate(character);
        var canvas = new PdfCanvas();
        canvas.Text(520, 585, character.EarlyChildhood, 30);
        canvas.Text(520, 649, character.LateChildhood, 30);
        canvas.Text(200, 713, $"School: {character.School}", 30);
        canvas.Text(200, 777, $"Basic field: {character.BasicSchool}", 30);
        canvas.Text(200, 841, $"Advanced field: {character.AdvancedSchool}", 30);
        canvas.Text(200, 905, $"Special field: {character.SpecialSchool}", 30);
        canvas.Text(200, 969,
            $"Careers: {string.Join(" -> ", character.RealLifeHistory)}", 30, 2050);
        canvas.Text(200, 1033,
            $"Homeworld: {character.HomePlanet}   Sex: {character.Sex}   Age: {character.Age}",
            24, 700);
        canvas.WrappedText(970, 585, character.Notes, 22, 1300, 64, 8);

        var row = 0;
        foreach (var weapon in character.Weapons)
        {
            if (row >= 16) break;
            var y = 1406 + row++ * 54;
            canvas.Text(200, y, $"{weapon.Count} x {weapon.Name}", 26, 950);
            canvas.Text(1220, y,
                $"Dmg: {weapon.Damage}, Ammo: {weapon.Shots}, Range: {weapon.Range}, Weight: {weapon.Mass}",
                22, 1100);
        }
        foreach (var item in character.Equipment)
        {
            if (row >= 16) break;
            var y = 1406 + row++ * 54;
            canvas.Text(200, y, $"{item.Count} x {item.Name}", 26, 950);
            canvas.Text(1220, y,
                $"Armor: {item.Armor}, Weight: {item.Mass}, {item.Notes}",
                22, 1100);
        }
        canvas.Text(300, 2388, summary.RemainingCBills.ToString(), 30);
        canvas.Text(1670, 435,
            $"Char# {CharacterId(character):000000000}", 20);
        return canvas.ToString();
    }

    private static string BuildAdditional(
        Character character,
        ResourceCatalog catalog)
    {
        var canvas = new PdfCanvas();
        canvas.Text(300, 617, character.Name, 30);
        canvas.Text(340, 693, character.Height.ToString(), 30);
        canvas.Text(1000, 693, character.Weight.ToString(), 30);
        canvas.Text(1550, 693,
            Join(character.Affiliation, character.SubAffiliation), 20);
        canvas.Text(320, 769, character.HairColor, 30);
        canvas.Text(960, 769, character.EyeColor, 30);

        var traits = character.Traits.OrderBy(item => item.Name).Skip(8).Take(28).ToArray();
        for (var index = 0; index < traits.Length; index++)
        {
            var secondColumn = index >= 14;
            var row = secondColumn ? index - 14 : index;
            var y = 1006 + row * 54;
            WriteTrait(canvas, traits[index], catalog,
                secondColumn ? 1275 : 150,
                secondColumn ? 1930 : 780,
                secondColumn ? 2030 : 880,
                secondColumn ? 2324 : 1177,
                y);
        }

        var skills = character.Skills.OrderBy(item => item.Name).Skip(30).Take(38).ToArray();
        for (var index = 0; index < skills.Length; index++)
        {
            var secondColumn = index >= 19;
            var row = secondColumn ? index - 19 : index;
            var y = 2015 + row * 68;
            WriteSkill(canvas, skills[index], catalog,
                secondColumn ? 1280 : 150,
                secondColumn ? 1848 : 718,
                secondColumn ? 1930 : 800,
                secondColumn ? 2302 : 1179,
                y,
                character.Traits);
        }
        canvas.Text(1670, 435,
            $"Char# {CharacterId(character):000000000}", 20);
        return canvas.ToString();
    }

    private static int CharacterId(Character character)
    {
        const uint offset = 2166136261;
        const uint prime = 16777619;
        var hash = offset;
        var identity = string.Join("|",
            character.Name,
            character.Affiliation,
            character.SubAffiliation,
            character.HomePlanet);
        foreach (var value in Encoding.UTF8.GetBytes(identity))
        {
            hash ^= value;
            hash *= prime;
        }
        return (int)(hash % 1_000_000_000);
    }

    private static void WriteTrait(
        PdfCanvas canvas,
        NamedValue trait,
        ResourceCatalog catalog,
        double nameX,
        double levelX,
        double referenceX,
        double xpX,
        double y)
    {
        var baseName = trait.Name.Split('/')[0];
        var reference = catalog.Traits
            .FirstOrDefault(item => item.Name == baseName)?.Reference ?? "";
        canvas.Text(nameX, y, trait.Name, 27, 550);
        canvas.Text(levelX, y,
            CharacterRules.TraitLevel(trait.Name, trait.Value).ToString(), 30);
        canvas.Text(referenceX, y, reference, 18, 170);
        canvas.TextRight(xpX, y, trait.Value.ToString(), 30);
    }

    private static void WriteSkill(
        PdfCanvas canvas,
        NamedValue skill,
        ResourceCatalog catalog,
        double nameX,
        double levelX,
        double rulesX,
        double xpX,
        double y,
        IEnumerable<NamedValue> traits)
    {
        var baseName = skill.Name.Split('/')[0];
        var rules = catalog.Skills
            .FirstOrDefault(item => item.Name.Split('/')[0] == baseName)?.Rules ?? "";
        canvas.Text(nameX, y, skill.Name, 27, 550);
        canvas.Text(levelX, y,
            CharacterRules.SkillLevel(skill.Value, traits).ToString(), 30);
        canvas.Text(rulesX, y, rules, 18, 220);
        canvas.TextRight(xpX, y, skill.Value.ToString(), 30);
    }

    private static int Find(IEnumerable<NamedValue> values, string name) =>
        values.FirstOrDefault(item => item.Name == name)?.Value ?? 0;

    private static string Join(string first, string second) =>
        string.Join(" / ", new[] { first, second }
            .Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string[] Split(string value) =>
        value.Split('/', StringSplitOptions.RemoveEmptyEntries |
            StringSplitOptions.TrimEntries);

    private static SheetImage LoadImage(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                "The official character sheet artwork is missing.", path);
        }
        var decoder = BitmapDecoder.Create(
            new Uri(path), BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);
        var source = decoder.Frames[0];
        var converted = new FormatConvertedBitmap(
            source, PixelFormats.Rgb24, null, 0);
        var stride = converted.PixelWidth * 3;
        var pixels = new byte[stride * converted.PixelHeight];
        converted.CopyPixels(pixels, stride, 0);
        using var compressed = new MemoryStream();
        using (var deflate = new ZLibStream(
                   compressed, CompressionLevel.SmallestSize, true))
        {
            deflate.Write(pixels);
        }
        return new SheetImage(
            converted.PixelWidth,
            converted.PixelHeight,
            compressed.ToArray());
    }

    private static byte[] BuildPdf(IReadOnlyList<SheetPage> pages)
    {
        var objectCount = 3 + pages.Count * 3;
        var objects = new byte[objectCount + 1][];
        objects[1] = Ascii("<< /Type /Catalog /Pages 2 0 R >>");
        var pageIds = Enumerable.Range(0, pages.Count)
            .Select(index => 6 + index * 3)
            .ToArray();
        objects[2] = Ascii(
            $"<< /Type /Pages /Count {pages.Count} /Kids " +
            $"[{string.Join(" ", pageIds.Select(id => $"{id} 0 R"))}] >>");
        objects[3] = Ascii(
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica " +
            "/Encoding /WinAnsiEncoding >>");

        for (var index = 0; index < pages.Count; index++)
        {
            var imageId = 4 + index * 3;
            var contentId = imageId + 1;
            var pageId = imageId + 2;
            var page = pages[index];
            objects[imageId] = StreamObject(
                $"<< /Type /XObject /Subtype /Image " +
                $"/Width {page.Image.Width} /Height {page.Image.Height} " +
                "/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /FlateDecode",
                page.Image.Data);
            var content = Ascii(
                "q\n612 0 0 792 0 0 cm\n/Im1 Do\nQ\n" + page.Content);
            objects[contentId] = StreamObject("<<", content);
            objects[pageId] = Ascii(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] " +
                $"/Resources << /Font << /F1 3 0 R >> " +
                $"/XObject << /Im1 {imageId} 0 R >> >> " +
                $"/Contents {contentId} 0 R >>");
        }

        using var output = new MemoryStream();
        Write(output, "%PDF-1.4\n%\xE2\xE3\xCF\xD3\n");
        var offsets = new long[objectCount + 1];
        for (var id = 1; id <= objectCount; id++)
        {
            offsets[id] = output.Position;
            Write(output, $"{id} 0 obj\n");
            output.Write(objects[id]);
            Write(output, "\nendobj\n");
        }
        var xref = output.Position;
        Write(output, $"xref\n0 {objectCount + 1}\n");
        Write(output, "0000000000 65535 f \n");
        for (var id = 1; id <= objectCount; id++)
        {
            Write(output, $"{offsets[id]:0000000000} 00000 n \n");
        }
        Write(output,
            $"trailer\n<< /Size {objectCount + 1} /Root 1 0 R >>\n" +
            $"startxref\n{xref}\n%%EOF\n");
        return output.ToArray();
    }

    private static byte[] StreamObject(string dictionary, byte[] content)
    {
        using var stream = new MemoryStream();
        Write(stream, $"{dictionary} /Length {content.Length} >>\nstream\n");
        stream.Write(content);
        Write(stream, "\nendstream");
        return stream.ToArray();
    }

    private static byte[] Ascii(string value) =>
        Encoding.Latin1.GetBytes(value);

    private static void Write(Stream stream, string value) =>
        stream.Write(Ascii(value));

    private sealed record SheetImage(int Width, int Height, byte[] Data);
    private sealed record SheetPage(SheetImage Image, string Content);

    private sealed class PdfCanvas
    {
        private readonly StringBuilder content = new();

        public void Text(
            double imageX,
            double imageY,
            string? value,
            double pixelSize,
            double maxPixelWidth = double.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var text = Sanitize(value);
            var fontSize = pixelSize * PageWidth / SheetWidth;
            var maxChars = maxPixelWidth == double.MaxValue
                ? int.MaxValue
                : Math.Max(1, (int)(maxPixelWidth / (pixelSize * 0.52)));
            if (text.Length > maxChars)
            {
                text = text[..Math.Max(1, maxChars - 1)] + "...";
            }
            var x = imageX * PageWidth / SheetWidth;
            var y = PageHeight -
                (imageY - TextBaselineLift) * PageHeight / SheetHeight;
            content.Append("BT /F1 ")
                .Append(F(fontSize))
                .Append(" Tf 0 g 1 0 0 1 ")
                .Append(F(x)).Append(' ').Append(F(y))
                .Append(" Tm (").Append(Escape(text)).Append(") Tj ET\n");
        }

        public void TextRight(
            double imageRightX,
            double imageY,
            string? value,
            double pixelSize)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var text = Sanitize(value);
            var width = text.Sum(character =>
                character == '-' ? pixelSize * 0.333 : pixelSize * 0.556);
            Text(imageRightX - width, imageY, text, pixelSize);
        }

        public void WrappedText(
            double imageX,
            double imageY,
            string? value,
            double pixelSize,
            double maxPixelWidth,
            double lineHeight,
            int maxLines)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var maxChars = Math.Max(8,
                (int)(maxPixelWidth / (pixelSize * 0.52)));
            var words = Sanitize(value)
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var lines = new List<string>();
            var line = new StringBuilder();
            foreach (var word in words)
            {
                if (line.Length > 0 && line.Length + word.Length + 1 > maxChars)
                {
                    lines.Add(line.ToString());
                    line.Clear();
                    if (lines.Count == maxLines) break;
                }
                if (line.Length > 0) line.Append(' ');
                line.Append(word);
            }
            if (line.Length > 0 && lines.Count < maxLines)
            {
                lines.Add(line.ToString());
            }
            for (var index = 0; index < lines.Count; index++)
            {
                Text(imageX, imageY + index * lineHeight,
                    lines[index], pixelSize, maxPixelWidth);
            }
        }

        public override string ToString() => content.ToString();

        private static string F(double value) =>
            value.ToString("0.###", CultureInfo.InvariantCulture);

        private static string Escape(string value) =>
            value.Replace("\\", "\\\\", StringComparison.Ordinal)
                .Replace("(", "\\(", StringComparison.Ordinal)
                .Replace(")", "\\)", StringComparison.Ordinal);

        private static string Sanitize(string value)
        {
            var builder = new StringBuilder(value.Length);
            foreach (var character in value.Replace('\r', ' ').Replace('\n', ' '))
            {
                builder.Append(character is >= ' ' and <= '~'
                    ? character
                    : character switch
                    {
                        '–' or '—' => '-',
                        '‘' or '’' => '\'',
                        '“' or '”' => '"',
                        _ => '?'
                    });
            }
            return builder.ToString();
        }
    }
}

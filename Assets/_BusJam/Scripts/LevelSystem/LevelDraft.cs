using System;
using System.Text;
using BusJam.Core;

namespace BusJam.LevelSystem
{
    // Level Editorde üzerinde çalışılan level
    // Bütün düzenleme işlemleri burada ve ekran sadece bunu çizip metotlarını çağırıyor
    
    public sealed class LevelDraft
    {
        // Satırları değiştirilebilir char dizileri olarak tutuyoruz ve string'ler değiştirilemiyor
        private char[][] _rows;

        public string Id { get; }
        public string DisplayName { get; }
        public int Width => _rows[0].Length;
        public int Height => _rows.Length;
        public int TimeLimitSeconds { get; private set; }
        public int WaitingSlotCount { get; private set; }
        public int BusCapacity { get; private set; }
        public string Buses { get; private set; }

        private LevelDraft(string id, char[][] rows, int time, int slots, int capacity, string buses)
        {
            Id = id;
            DisplayName = DisplayNameFromId(id);
            _rows = rows;
            TimeLimitSeconds = time;
            WaitingSlotCount = slots;
            BusCapacity = capacity;
            Buses = buses;
        }

        public static LevelDraft CreateNew(string id, int width = 4, int height = 4)
        {
            var rows = new char[height][];
            for (int y = 0; y < height; y++)
                rows[y] = NewRow(width);

            var defaults = new LevelData();
            return new LevelDraft(id, rows, defaults.timeLimitSeconds, defaults.waitingSlotCount, defaults.busCapacity, "");
        }

        public static LevelDraft FromLevel(LevelData level)
        {
            var rows = new char[level.Height][];
            for (int y = 0; y < rows.Length; y++)
                rows[y] = level.rows[y].ToCharArray();

            return new LevelDraft(level.id, rows, level.timeLimitSeconds, level.waitingSlotCount, level.busCapacity, level.buses ?? "");
        }

        public char GetCell(int x, int y) => _rows[y][x];

        public void Paint(int x, int y, char cell)
        {
            if (!LevelFormat.IsKnownCell(cell))
                throw new ArgumentException($"Unknown cell '{cell}'.", nameof(cell));

            _rows[y][x] = cell;
        }

        // Boyut değişince var olan hücreler yerinde kalıyor sonra da yeni hücreler boş geliyor
        public void Resize(int width, int height)
        {
            width = Math.Clamp(width, 1, LevelValidator.MaxWidth);
            height = Math.Clamp(height, 1, LevelValidator.MaxHeight);

            var rows = new char[height][];
            for (int y = 0; y < height; y++)
            {
                rows[y] = NewRow(width);
                if (y >= Height)
                    continue;

                int copy = Math.Min(width, Width);
                Array.Copy(_rows[y], rows[y], copy);
            }
            _rows = rows;
        }

        // Kullanıcı ne yazarsa yazsın sadece geçerli renk harflerini tutuyoruz
        public void SetBuses(string text)
        {
            var result = new StringBuilder();
            if (text != null)
            {
                foreach (char c in text.ToUpperInvariant())
                {
                    if (LevelFormat.TryParseColor(c, out _))
                        result.Append(c);
                }
            }
            Buses = result.ToString();
        }

        public void SetTimeLimit(int seconds) =>
            TimeLimitSeconds = Math.Clamp(seconds, 1, LevelValidator.MaxTimeLimitSeconds);

        public void SetWaitingSlots(int count) =>
            WaitingSlotCount = Math.Clamp(count, 1, LevelValidator.MaxWaitingSlots);

        // Editörde her renk için kaç yolcu var, kaç koltuk var gösterebilmek için
        public int CountPassengers(PassengerColor color)
        {
            int count = 0;
            foreach (char[] row in _rows)
            {
                foreach (char c in row)
                {
                    if (LevelFormat.TryParseColor(c, out PassengerColor cellColor) && cellColor == color)
                        count++;
                }
            }
            return count;
        }

        public int CountBuses(PassengerColor color)
        {
            int count = 0;
            foreach (char c in Buses)
            {
                if (LevelFormat.TryParseColor(c, out PassengerColor busColor) && busColor == color)
                    count++;
            }
            return count;
        }

        public LevelData ToLevelData()
        {
            var rows = new string[Height];
            for (int y = 0; y < Height; y++)
                rows[y] = new string(_rows[y]);

            return new LevelData
            {
                id = Id,
                displayName = DisplayName,
                timeLimitSeconds = TimeLimitSeconds,
                busCapacity = BusCapacity,
                waitingSlotCount = WaitingSlotCount,
                rows = rows,
                buses = Buses,
            };
        }

        // level_007 yi Level 7, beklenmedik bir isimse olduğu gibi kullanıyoruz
        private static string DisplayNameFromId(string id)
        {
            const string prefix = "level_";
            if (id.StartsWith(prefix) && int.TryParse(id.Substring(prefix.Length), out int number))
                return $"Level {number}";
            return id;
        }

        private static char[] NewRow(int width)
        {
            var row = new char[width];
            for (int x = 0; x < width; x++)
                row[x] = LevelFormat.EmptyCell;
            return row;
        }
    }
}
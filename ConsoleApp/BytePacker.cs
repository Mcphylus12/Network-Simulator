namespace ConsoleApp;

public class BytePacker
{
    private readonly uint _size;
    private readonly Dictionary<string, PackerSection> _packerSections;

    public BytePacker(IEnumerable<PackerSection> packerSections)
    {
        uint offset = 0;

        foreach (var item in packerSections)
        {
            item.Offset = offset;
            offset += item.Size;
        }

        _size = offset;
        _packerSections = packerSections.ToDictionary(ps => ps.Name);
    }

    public byte[] CreatePacket(uint variableLength = 0)
    {
        return new byte[_size + variableLength];
    }

    public T Get<T>(byte[] bytes, string name)
    {
        var section = GetSection(name);
        var byteSection = section.Size == 0 ? bytes[(int)section.Offset..] : bytes[(int)section.Offset..(int)(section.Offset + section.Size)];

        return (T)section.FromBytes(byteSection);
    }

    private PackerSection GetSection(string name)
    {
        if (!_packerSections.ContainsKey(name))
        {
            throw new ArgumentException("Not a valid section", nameof(name));
        }

        var section = _packerSections[name];
        return section;
    }

    public void Set<T>(byte[] bytes, string name, T value)
    {
        if (value is null)
        {
            throw new ArgumentException("Value cannot be null", nameof(value));
        }

        var section = GetSection(name);

        if (section is not PackerSection<T>)
        {
            throw new ArgumentException("Invalid value Type");
        }

        var byteSection = section.ToBytes(value);

        Array.Copy(byteSection, 0, bytes, section.Offset, section.Size > 0 ? section.Size : byteSection.Length);
    }

    public abstract class PackerSection
    {
        public uint Offset { get; set; }
        public uint Size { get; }
        public string Name { get; }

        protected PackerSection(string name, uint size)
        {
            Name = name;
            Size = size;
        }

        internal abstract object FromBytes(byte[] byteSection);

        internal abstract byte[] ToBytes(object value);
    }

    public class PackerSection<T> : PackerSection
    {
        private readonly Func<T, byte[]>? _toBytes;
        private readonly Func<byte[], T>? _fromBytes;

        public PackerSection(string name, uint size, Func<T, byte[]>? toBytes = null, Func<byte[], T>? fromBytes = null)
            : base(name, size)
        {
            if (typeof(T) != typeof(byte[]) && (toBytes is null || fromBytes is null))
            {
                throw new ArgumentException("converters can only be null for byte arrays");
            }

            _toBytes = toBytes;
            _fromBytes = fromBytes;
        }

        internal override object FromBytes(byte[] byteSection) => _fromBytes is null ? byteSection : _fromBytes(byteSection)!;

        internal override byte[] ToBytes(object value) => _toBytes is null ? (byte[])value : _toBytes((T)value);
    }
}

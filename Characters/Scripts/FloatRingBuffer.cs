public class FloatRingBuffer : RingBuffer<float>
{
    public FloatRingBuffer(int capacity) : base(capacity) {}

    public float Max() {
        if (Count == 0) return float.NaN;
        float m = float.MinValue;
        foreach (float v in this) {
            if (v > m)
                m = v;
        }
        return m;
    }
    
    public float Min() {
        if (Count == 0) return float.NaN;
        float m = float.MaxValue;
        foreach (float v in this) {
            if (v < m)
                m = v;
        }
        return m;
    }
}
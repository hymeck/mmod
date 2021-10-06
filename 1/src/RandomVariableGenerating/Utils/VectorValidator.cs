namespace RandomVariableGenerating.Utils
{
    public class VectorValidator
    {
        private readonly int[] _vector;

        public VectorValidator(int[] vector)
        {
            _vector = vector;
        }
        
        public bool Validate()
        {
            if (_vector == null || _vector.Length == 0)
                return false;

            if (_vector.Length == 1)
                return true;

            var index = 0;
            while (index <= _vector.Length - 2)
            {
                if (_vector[index] < _vector[index++ + 1])
                    continue;
                return false;
            }

            return true;
        }

        public static bool Validate(int[] vector) => new VectorValidator(vector).Validate();
    }
}

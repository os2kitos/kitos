using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions.Extensions;

namespace Core.Abstractions.Types
{
    public class EnumMap<TLeft, TRight>
        where TLeft : Enum
        where TRight : Enum
    {
        private readonly IReadOnlyDictionary<TLeft, TRight> _leftToRightMap;
        private readonly IReadOnlyDictionary<TRight, TLeft> _rightToLeftMap;

        public EnumMap(params (TLeft left, TRight right)[] mappings)
        {
            _leftToRightMap = mappings
                .ToDictionary(x => x.left, x => x.right)
                .AsReadOnly();

            _rightToLeftMap = _leftToRightMap
                .ToDictionary(kvp => kvp.Value, kvp => kvp.Key)
                .AsReadOnly();
        }

        public TRight FromLeftToRight(TLeft value)
        {
            return _leftToRightMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped 'Left'->'Right':{value:G}", nameof(value));
        }

        public TLeft FromRightToLeft(TRight value)
        {
            return _rightToLeftMap.TryGetValue(value, out var result)
                ? result
                : throw new ArgumentException($@"Unmapped 'Right'->'Left' value:{value:G}", nameof(value));
        }
    }
}

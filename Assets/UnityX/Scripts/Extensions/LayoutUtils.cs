using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LayoutUtils {
    // Wraps layout item params and the layouts to apply the output to, if any.
    public class LayoutItem {
        public LayoutItemParams layoutItemParams;
        // This may be null.
        public List<SLayout> layouts;
        
        public LayoutItem(LayoutItemParams layoutItemParams, params SLayout[] layouts) {
            this.layoutItemParams = layoutItemParams;
            if(layouts != null && layouts.Length > 0) this.layouts = new List<SLayout>(layouts);
        }

        // Not sure if this is a good idea or not!
        // public static LayoutItem Fixed(SLayout icon, int axis) {
        //     return new LayoutItem(LayoutItemParams.Fixed(icon.size[axis]), icon);
        // }
    }

    [System.Serializable]
    public struct LayoutItemParams {
        public bool flexible;
        public float fixedSize;
        public float minSize;
        public float maxSize;
        public float weight;

        public static LayoutItemParams Fixed(float size) {
            var layoutItem = new LayoutItemParams {
                fixedSize = size
            };
            return layoutItem;
        }
		
        public static LayoutItemParams Flexible(float minSize = 0, float maxSize = float.MaxValue, float weight = 1) {
            var layoutItem = new LayoutItemParams {
                flexible = true,
                minSize = minSize,
                maxSize = maxSize,
                weight = weight
            };
            return layoutItem;
        }
    }
    

    public static List<Vector2> GetLayoutRanges(float containerWidth, List<LayoutItemParams> items, float spacing = 0, float pivot = 0.5f) {
        float fixedTotal = items.Where(i => !i.flexible).Sum(i => i.fixedSize);
        float initialFlexSpace = items.Where(i => i.flexible).Sum(i => i.minSize);
        float totalSpacing = spacing * (items.Count - 1);
        float availableFlexibleSpace = containerWidth - fixedTotal - initialFlexSpace - totalSpacing;

        // Map to hold final sizes for each flexible item
        var flexItemSizes = items.Where(i => i.flexible).ToDictionary(i => i, i => i.minSize);

        while (availableFlexibleSpace > 0) {
            float totalWeight = items.Where(i => i.flexible && flexItemSizes[i] < i.maxSize).Sum(i => i.weight);

            if (totalWeight == 0) break;

            float spaceAllocatedThisIteration = 0;

            foreach (var item in items.Where(i => i.flexible)) {
                if (flexItemSizes[item] >= item.maxSize)
                    continue; 

                float weightFraction = item.weight / totalWeight;
                float spaceForThisItem = weightFraction * availableFlexibleSpace;
                float spaceActuallyUsed = Math.Min(spaceForThisItem, item.maxSize - flexItemSizes[item]);
                
                flexItemSizes[item] += spaceActuallyUsed;
                spaceAllocatedThisIteration += spaceActuallyUsed;
            }

            // Reduce the available space by the space that was allocated in this iteration
            availableFlexibleSpace -= spaceAllocatedThisIteration;
        }

        float totalSizeWithSpacing = fixedTotal + flexItemSizes.Values.Sum() + totalSpacing;
        float offset = (containerWidth - totalSizeWithSpacing) * pivot;
        float currentPosition = offset;

        var ranges = new List<Vector2>();
		
        foreach (var item in items) {
            float itemSize = item.flexible ? flexItemSizes[item] : item.fixedSize;
            ranges.Add(new Vector2(currentPosition, currentPosition + itemSize));
            currentPosition += itemSize + spacing;
        }

        return ranges;
    }
}
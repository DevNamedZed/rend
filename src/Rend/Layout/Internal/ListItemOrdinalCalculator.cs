using Rend.Css;
using Rend.Css.Properties.Internal;
using Rend.Style;

namespace Rend.Layout.Internal
{
    /// <summary>
    /// Computes the list-item counter ordinal for a list-item box, honoring
    /// &lt;ol start&gt;, &lt;ol reversed&gt;, &lt;li value&gt;, and CSS
    /// counter-increment / counter-set on the list-item counter.
    /// </summary>
    /// <spec>CSS-LISTS-3 §3 https://drafts.csswg.org/css-lists/#instantiating-counters</spec>
    /// <spec>HTML §4.4.8 https://html.spec.whatwg.org/multipage/grouping-content.html#the-ol-element</spec>
    internal static class ListItemOrdinalCalculator
    {
        private const string ListItemCounterName = "list-item";

        public static int Compute(LayoutBox itemBox)
        {
            LayoutBox? parent = itemBox.Parent;
            if (parent == null)
            {
                return 1;
            }

            ListContainerInfo containerInfo = ReadContainerInfo(parent);
            int counter = ComputeInitialCounter(parent, containerInfo);

            for (int i = 0; i < parent.Children.Count; i++)
            {
                LayoutBox sibling = parent.Children[i];
                if (sibling.BoxType != BoxType.ListItem)
                {
                    continue;
                }

                counter = ApplyItemCounterChanges(counter, sibling, containerInfo);

                if (ReferenceEquals(sibling, itemBox))
                {
                    return counter;
                }
            }

            return counter;
        }

        /// <summary>
        /// Computes the list-item ordinal during layout, when the current
        /// item has not yet been added to its parent's LayoutBox.Children
        /// list. Callable from inside layout contexts that need the marker
        /// text before the item's laid-out siblings are complete.
        /// </summary>
        public static int ComputeAtLayoutTime(LayoutBox itemBox)
        {
            LayoutBox? parent = itemBox.Parent;
            if (parent == null)
            {
                return 1;
            }

            ListContainerInfo containerInfo = ReadContainerInfo(parent);
            int counter = ComputeInitialCounterForLayoutTime(parent, containerInfo);

            for (int i = 0; i < parent.Children.Count; i++)
            {
                LayoutBox sibling = parent.Children[i];
                if (sibling.BoxType != BoxType.ListItem)
                {
                    continue;
                }

                counter = ApplyItemCounterChanges(counter, sibling, containerInfo);
            }

            counter = ApplyItemCounterChanges(counter, itemBox, containerInfo);
            return counter;
        }

        private static int ComputeInitialCounterForLayoutTime(LayoutBox parent, ListContainerInfo containerInfo)
        {
            if (containerInfo.HasStart)
            {
                return containerInfo.StartValue - containerInfo.DefaultIncrement;
            }
            if (containerInfo.IsReversed)
            {
                // At layout time not all list-item siblings are present in
                // parent.Children. Fall back to the styled tree to determine
                // the total item count used by Chrome's reversed logic.
                int itemCount = CountListItemsInStyledTree(parent);
                return itemCount + 1;
            }
            return 0;
        }

        private static int CountListItemsInStyledTree(LayoutBox parent)
        {
            if (parent.StyledNode is not StyledElement parentStyled)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < parentStyled.Children.Count; i++)
            {
                if (parentStyled.Children[i] is StyledElement child
                    && child.Style.Display == CssDisplay.ListItem)
                {
                    count++;
                }
            }
            return count;
        }

        private static int ComputeInitialCounter(LayoutBox parent, ListContainerInfo containerInfo)
        {
            if (containerInfo.HasStart)
            {
                // First list item's default increment should land on the start value.
                return containerInfo.StartValue - containerInfo.DefaultIncrement;
            }
            if (containerInfo.IsReversed)
            {
                // Chrome uses count+1 so the default -1 increment on the first item lands on count.
                // Note: this ignores explicit counter-increment / counter-set on list items,
                // matching Chrome's actual behavior (not the CSS Lists spec §4.4.2 algorithm,
                // which Chrome does not fully implement).
                int itemCount = CountListItems(parent);
                return itemCount + 1;
            }
            return 0;
        }

        private static int CountListItems(LayoutBox parent)
        {
            int count = 0;
            for (int i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i].BoxType == BoxType.ListItem)
                {
                    count++;
                }
            }
            return count;
        }

        private static int ApplyItemCounterChanges(int counter, LayoutBox itemBox, ListContainerInfo containerInfo)
        {
            int increment = ReadCounterIncrement(itemBox, defaultValue: containerInfo.DefaultIncrement);
            counter += increment;

            // [CSS-LISTS-3 §2.3] counter-set is applied after counter-increment.
            if (TryReadCounterSet(itemBox, out int setValue))
            {
                return setValue;
            }

            if (itemBox.StyledNode is StyledElement itemElement)
            {
                string? valueAttribute = itemElement.Element?.GetAttribute("value");
                if (valueAttribute != null && int.TryParse(valueAttribute, out int attributeValue))
                {
                    return attributeValue;
                }
            }

            return counter;
        }

        private static ListContainerInfo ReadContainerInfo(LayoutBox parent)
        {
            bool reversed = false;
            bool hasStart = false;
            int startValue = 0;

            if (parent.StyledNode is StyledElement parentElement)
            {
                var element = parentElement.Element;
                if (element?.GetAttribute("reversed") != null)
                {
                    reversed = true;
                }
                string? startAttribute = element?.GetAttribute("start");
                if (startAttribute != null && int.TryParse(startAttribute, out int parsedStart))
                {
                    hasStart = true;
                    startValue = parsedStart;
                }
            }

            return new ListContainerInfo(reversed, hasStart, startValue);
        }

        private static int ReadCounterIncrement(LayoutBox itemBox, int defaultValue)
        {
            if (itemBox.StyledNode?.Style is ComputedStyle style)
            {
                object? raw = style.GetRefValue(PropertyId.CounterIncrement);
                if (TryFindListItemCounter(raw, defaultForMissingNumber: 1, out int value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        private static bool TryReadCounterSet(LayoutBox itemBox, out int value)
        {
            value = 0;
            if (itemBox.StyledNode?.Style is ComputedStyle style)
            {
                object? raw = style.GetRefValue(PropertyId.CounterSet);
                return TryFindListItemCounter(raw, defaultForMissingNumber: 0, out value);
            }
            return false;
        }

        private static bool TryFindListItemCounter(object? raw, int defaultForMissingNumber, out int value)
        {
            value = 0;
            if (raw == null)
            {
                return false;
            }
            if (raw is CssKeywordValue keyword)
            {
                if (keyword.Keyword == "none")
                {
                    return false;
                }
                if (keyword.Keyword == ListItemCounterName)
                {
                    value = defaultForMissingNumber;
                    return true;
                }
                return false;
            }
            if (raw is CssListValue list)
            {
                for (int i = 0; i < list.Values.Count; i++)
                {
                    if (list.Values[i] is CssKeywordValue entryKeyword && entryKeyword.Keyword == ListItemCounterName)
                    {
                        if (i + 1 < list.Values.Count && list.Values[i + 1] is CssNumberValue number)
                        {
                            value = (int)number.Value;
                            return true;
                        }
                        value = defaultForMissingNumber;
                        return true;
                    }
                }
            }
            return false;
        }

        private readonly struct ListContainerInfo
        {
            public bool IsReversed { get; }
            public bool HasStart { get; }
            public int StartValue { get; }

            public ListContainerInfo(bool isReversed, bool hasStart, int startValue)
            {
                IsReversed = isReversed;
                HasStart = hasStart;
                StartValue = startValue;
            }

            public int DefaultIncrement => IsReversed ? -1 : 1;
        }
    }
}

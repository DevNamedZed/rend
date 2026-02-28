namespace Rend.Layout
{
    /// <summary>
    /// The formatting context type for a layout box.
    /// </summary>
    public enum BoxType : byte
    {
        Block,
        Inline,
        InlineBlock,
        Flex,
        Grid,
        Table,
        TableRow,
        TableCell,
        TableRowGroup,
        TableColumn,
        TableColumnGroup,
        TableCaption,
        ListItem,
        None
    }
}

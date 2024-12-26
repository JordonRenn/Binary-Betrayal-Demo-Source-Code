public static class DS_NodeFactory
{
    public static DS_Node CreateNode(DS_DialogueType type)
    {
        switch (type)
        {
            case DS_DialogueType.SingleChoice:
                return new DS_SingleChoiceNode();
            case DS_DialogueType.MultiChoice:
                return new DS_MultiChoiceNode();
            default:
                return null;
        }
    }
}

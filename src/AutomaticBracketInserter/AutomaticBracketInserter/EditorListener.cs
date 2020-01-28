using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace AutomaticBracketInserter
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class EditorListener : IWpfTextViewCreationListener
    {
        [Import] private readonly IAsyncCompletionBroker _completionBroker = null;
        [Import] private readonly IEditorOperationsFactoryService _editorOperationsFactory = null;

        private ITextView TextView { get; set; }
        private IEditorOperations EditorOperations { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            if (_completionBroker != null)
            {
                _completionBroker.CompletionTriggered += CompletionBroker_CompletionTriggered;
            }
        }

        private void CompletionBroker_CompletionTriggered(object sender, CompletionTriggeredEventArgs eventArgs)
        {
            eventArgs.CompletionSession.ItemCommitted += CompletionSession_ItemCommitted;

            TextView = eventArgs.TextView;
        }

        private void CompletionSession_ItemCommitted(object sender, CompletionItemEventArgs itemEventArgs)
        {
            if (TryIsChooseMethod(itemEventArgs))
            {
                EditorOperations = _editorOperationsFactory.GetEditorOperations(TextView);
                string parenthesis = "()";
                EditorOperations.InsertText(parenthesis);
            }
        }

        private bool TryIsChooseMethod(CompletionItemEventArgs itemEventArgs)
        {
            if (itemEventArgs.Item.Filters.IsEmpty)
            {
                return false;
            }

            var allowedMethods = new Dictionary<string, string>() { { "m", "Methods" }, { "x", "Extension methods" } };
            foreach (var item in itemEventArgs.Item.Filters)
            {
                if (allowedMethods.ContainsKey(item.AccessKey) && allowedMethods[item.AccessKey] == item.DisplayText)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

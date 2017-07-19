package com.jetbrains.rider.plugins.fsharp.services.fsi

import com.intellij.openapi.editor.Editor
import com.intellij.psi.PsiFile
import com.intellij.util.DocumentUtil

class SendToFsiActionExecutor(val consoleRunner: FsiConsoleRunner) {
    fun execute(editor: Editor, file: PsiFile) {
        val hasSelection = editor.selectionModel.hasSelection()
        val visibleText = getTextToSend(editor, hasSelection)
        if (!visibleText.isEmpty()) {
            val fsiText = "\n" +
                    "# silentCd @\"${file.containingDirectory.virtualFile.path}\" ;; \n" +
                    "# ${getTextStartLine(editor, hasSelection) + 1} @\"${file.virtualFile.path}\" \n" +
                    visibleText + "\n" +
                    "# 1 \"stdin\"\n;;\n"
            consoleRunner.sendText(visibleText, fsiText)
        }
        if (!hasSelection && consoleRunner.fsiHost.moveCaretOnSendLine)
            editor.caretModel.moveCaretRelatively(0, 1, false, false, true)
    }

    fun getTextToSend(editor: Editor, hasSelection: Boolean) =
            if (hasSelection) editor.selectionModel.selectedText!!
            else editor.document.getText(DocumentUtil.getLineTextRange(editor.document, editor.caretModel.logicalPosition.line))

    fun getTextStartLine(editor: Editor, hasSelection: Boolean) =
            if (hasSelection) editor.document.getLineNumber(editor.selectionModel.selectionStart)
            else editor.caretModel.logicalPosition.line
}
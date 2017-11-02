package com.jetbrains.rider.plugins.fsharp.services.fsi

import com.intellij.icons.AllIcons
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.command.WriteCommandAction
import com.intellij.openapi.project.DumbAwareAction
import com.intellij.openapi.ui.popup.JBPopupFactory
import com.intellij.openapi.ui.popup.PopupStep
import com.intellij.openapi.ui.popup.util.BaseListPopupStep

class CommandHistoryAction(private val consoleRunner: FsiConsoleRunner)
    : DumbAwareAction(title, null, AllIcons.Vcs.History) {
    companion object {
        val title = "Set recent command"
    }

    private val consoleView = consoleRunner.consoleView
    private val commandHistory = consoleRunner.commandHistory

    override fun actionPerformed(e: AnActionEvent) {
        val entries = consoleRunner.commandHistory.entries.reversed()
        val popupList = object : BaseListPopupStep<CommandHistory.Entry>(title, entries) {
            override fun onChosen(selectedValue: CommandHistory.Entry, finalChoice: Boolean): PopupStep<*>? {
                if (consoleRunner.fsiHost.copyRecentToEditor)
                    WriteCommandAction.runWriteCommandAction(consoleView.project) {
                        consoleView.editorDocument.setText(selectedValue.visibleText)
                    }
                else
                    consoleRunner.fsiHost.sendToFsi(selectedValue.visibleText, selectedValue.executableText)
                return PopupStep.FINAL_CHOICE
            }
        }

        val popup = JBPopupFactory.getInstance().createListPopup(popupList)
        val c = e.inputEvent?.component
        if (c != null) {
            popup.showUnderneathOf(c)
        } else {
            popup.showInBestPositionFor(e.dataContext)
        }
    }

    override fun update(e: AnActionEvent) {
        e.presentation.isEnabled = commandHistory.entries.isNotEmpty() && consoleView.isEditable
    }
}
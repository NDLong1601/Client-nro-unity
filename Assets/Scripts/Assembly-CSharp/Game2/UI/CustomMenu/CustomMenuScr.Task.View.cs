using System;
using System.Collections.Generic;
using Game2.Assets.src.g;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private void PaintTaskTabContent(mGraphics g)
        {
            PaintSubTabs(g);
            PaintTaskListColumn(g);
            PaintTaskDetailColumn(g);
        }

        private void PaintSubTabs(mGraphics g)
        {
            UiMenuTheme.PaintButton(g, _subTab0Rect, "Nhiệm vụ chính",
                _selectedSubTab == 0);
            UiMenuTheme.PaintButton(g, _subTab1Rect, "Nhiệm vụ khác",
                _selectedSubTab == 1);
        }

        private void PaintTaskListColumn(mGraphics g)
        {
            UiRect listViewport = GetLeftListViewport();
            UiMenuTheme.PaintSurface(g, listViewport);

            using (UiRenderState.Push(g, listViewport, clip: true))
            {
                int scrollY = (_leftScrollAdapter != null) ? _leftScrollAdapter.ScrollY : 0;
                int clipTop = _leftColRect.Y;
                int clipBottom = listViewport.Y + listViewport.Height;

                if (_selectedSubTab == 0)
                {
                    // Main Tasks
                    Task currentTask = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;
                    int currentTaskId = currentTask != null ? (int)currentTask.taskId : 0;
                    int currentTaskPosition = GetMainTaskPosition(currentTaskId);
                    int[] taskSequence = BuildMainTaskSequence();
                    int totalRows = GetMainTaskRowCount();
                    int numberColW = 34;
                    int maxTextW = _leftColRect.Width - numberColW - 35;
                    int textX = _leftColRect.X + numberColW + 6;

                    for (int i = 0; i < totalRows; i++)
                    {
                        int rowY = _leftColRect.Y + i * MainTaskRowHeight - scrollY;
                        if (rowY + MainTaskRowHeight <= clipTop || rowY >= clipBottom) continue;

                        bool isSelected = (i == _selectedTaskPosition);
                        int status;
                        if (i < currentTaskPosition) status = 0;
                        else if (i == currentTaskPosition) status = 1;
                        else status = 2;

                        UiMenuTheme.PaintCard(g,
                            new UiRect(_leftColRect.X + 1, rowY, _leftColRect.Width - 2, MainTaskRowHeight - 1),
                            isSelected ? 0xFFF0B0 : (status == 2 ? 0xD8D8D8 : 0xF6F3EE),
                            isSelected ? 0xF8CD63 : 0xB9AA93);

                        g.setColor(status == 2 ? 0xC4B9A8 : 0xB8A58B);
                        g.fillRect(_leftColRect.X + 1, rowY, numberColW, MainTaskRowHeight - 1);
                        mFont.tahoma_7b_dark.drawString(g, (i + 1).ToString(), _leftColRect.X + numberColW / 2, rowY + 8, mFont.CENTER);

                        // Task Title
                        int taskId = taskSequence[i];
                        string taskTitle = GetMainTaskName(taskId);

                        if (rowY + 4 >= clipTop && rowY + 4 < clipBottom)
                        {
                            string displayTitle = TruncateString(mFont.tahoma_7b_dark, taskTitle, maxTextW);
                            mFont.tahoma_7b_dark.drawString(g, displayTitle, textX, rowY + 3, mFont.LEFT);
                        }

                        // Status text & icons with strict bounds checking
                        bool iconVisible = (rowY + 6 >= clipTop && rowY + 26 <= clipBottom);
                        bool statusTextVisible = (rowY + 18 >= clipTop && rowY + 18 < clipBottom);

                        if (status == 0) // Done
                        {
                            if (statusTextVisible)
                                mFont.tahoma_7_green2.drawString(g, "Đã hoàn thành", textX, rowY + 18, mFont.LEFT);
                            if (iconVisible)
                            {
                                Image statusIcon = _statusIcons != null && _statusIcons.Length > 0 ? _statusIcons[0] : null;
                                if (statusIcon != null) g.drawImage(statusIcon, _leftColRect.X + _leftColRect.Width - 12, rowY + MainTaskRowHeight / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                                else DrawCheckmark(g, _leftColRect.X + _leftColRect.Width - 20, rowY + 10, 14);
                            }
                        }
                        else if (status == 1) // In Progress
                        {
                            if (statusTextVisible)
                            {
                                mFont progressFont = isSelected ? mFont.tahoma_7b_dark : mFont.tahoma_7_orange;
                                progressFont.drawString(g, "Đang thực hiện", textX, rowY + 17, mFont.LEFT);
                            }
                            if (rowY + 12 >= clipTop && rowY + 12 < clipBottom)
                            {
                                int pct = 0;
                                if (currentTask != null && currentTask.counts != null && currentTask.index < currentTask.counts.Length && currentTask.counts[currentTask.index] > 0)
                                {
                                    pct = currentTask.count * 100 / currentTask.counts[currentTask.index];
                                }
                                if (pct < 0) pct = 0;
                                if (pct > 100) pct = 100;
                                string pctStr = pct + "%";
                                mFont.tahoma_7b_dark.drawString(g, pctStr, _leftColRect.X + _leftColRect.Width - 8, rowY + 17, mFont.RIGHT);
                            }
                        }
                        else // Locked
                        {
                            if (statusTextVisible)
                                mFont.tahoma_7_grey.drawString(g, "Khóa", textX, rowY + 18, mFont.LEFT);
                            if (iconVisible)
                            {
                                Image statusIcon = _statusIcons != null && _statusIcons.Length > 1 ? _statusIcons[1] : null;
                                if (statusIcon != null) g.drawImage(statusIcon, _leftColRect.X + _leftColRect.Width - 12, rowY + MainTaskRowHeight / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                                else DrawLock(g, _leftColRect.X + _leftColRect.Width - 20, rowY + 10, 11, 14);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < _otherQuestCardRects.Length; i++) _otherQuestCardRects[i] = UiRect.Empty;
                    int groupY = _leftColRect.Y - scrollY;
                    groupY = PaintOtherQuestGroup(g, "Nhiệm vụ hằng ngày", new int[] { 0, 1 }, groupY);
                    groupY = PaintOtherQuestGroup(g, "Nhiệm vụ bang", new int[] { 3 }, groupY + 4);
                    PaintOtherQuestGroup(g, "Nhiệm vụ câu cá", new int[] { 2 }, groupY + 4);
                }
            }
        }

        private int PaintOtherQuestGroup(mGraphics g, string title, int[] questIndexes, int y)
        {
            mFont.tahoma_7b_dark.drawString(g, title, _leftColRect.X + 4, y, mFont.LEFT);
            y += 14;
            bool hasAny = false;
            for (int i = 0; i < questIndexes.Length; i++)
            {
                int questIndex = questIndexes[i];
                if (!allOtherQuests[questIndex].hasQuest) continue;
                PaintOtherQuestCard(g, questIndex, y);
                y += OtherTaskRowHeight + 1;
                hasAny = true;
            }
            if (!hasAny)
            {
                mFont.tahoma_7_grey.drawString(g, "--- Không có nhiệm vụ ---", _leftColRect.X + _leftColRect.Width / 2, y + 3, mFont.CENTER);
                y += 21;
            }
            return y;
        }

        private void PaintOtherQuestCard(mGraphics g, int questIndex, int y)
        {
            NpcQuest q = allOtherQuests[questIndex];
            UiRect rect = new UiRect(_leftColRect.X + 1, y, _leftColRect.Width - 2, OtherTaskRowHeight);
            _otherQuestCardRects[questIndex] = rect;
            bool selected = questIndex == _selectedOtherCategoryIndex;
            UiMenuTheme.PaintCard(g, rect,
                selected ? 0xFFF0B0 : 0xF6F3EE,
                selected ? 0xF8CD63 : 0xB9AA93);

            Image icon = _otherTaskIcons != null && q.iconType < _otherTaskIcons.Length ? _otherTaskIcons[q.iconType] : null;
            if (icon != null) g.drawImage(icon, rect.X + 17, rect.Y + rect.Height / 2, mGraphics.HCENTER | mGraphics.VCENTER);
            int textX = rect.X + 36;
            string displayTitle = TruncateString(mFont.tahoma_7b_dark, q.title, rect.Width - 74);
            mFont.tahoma_7b_dark.drawString(g, displayTitle, textX, rect.Y + 3, mFont.LEFT);
            (q.isComplete ? mFont.tahoma_7_green2 : mFont.tahoma_7_orange).drawString(g, q.isComplete ? "Đã hoàn thành" : "Đang thực hiện", textX, rect.Y + 17, mFont.LEFT);
            int percent = q.isComplete ? 100 : (q.maxCount > 0 ? q.count * 100 / q.maxCount : 0);
            if (percent < 0) percent = 0;
            if (percent > 100) percent = 100;
            mFont.tahoma_7b_dark.drawString(g, percent + "%", rect.X + rect.Width - 7, rect.Y + 10, mFont.RIGHT);
        }

        private void PaintTaskDetailColumn(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _rightColRect);
            UiMenuTheme.PaintHeader(g, new UiRect(_rightColRect.X + 2, _rightColRect.Y + 1,
                _rightColRect.Width - 4, 23), "Chi tiết nhiệm vụ");

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
                int y = _rightBodyRect.Y + 2 - scrollY;
                if (_selectedSubTab == 0) PaintMainTaskDetailBody(g, y);
                else PaintOtherTaskDetailBody(g, y);
            }
        }

        private void PaintMainTaskDetailBody(mGraphics g, int y)
        {
            int taskId = GetSelectedTaskId();
            int currentTaskPosition = GetMainTaskPosition(GetCurrentTaskId());
            int status = _selectedTaskPosition < currentTaskPosition ? 0 : (_selectedTaskPosition == currentTaskPosition ? 1 : 2);
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            int currentStep = currentTask != null && (int)currentTask.taskId == taskId ? currentTask.index : 0;
            int midX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            int bulletX = _rightBodyRect.X + 5;
            int textX = bulletX + 10;
            int maxTextW = _rightBodyRect.Width - 20;

            mFont.tahoma_7b_dark.drawString(g, GetMainTaskName(taskId), midX, y, mFont.CENTER);
            y += 18;
            DrawDetailDivider(g, y);
            y += 7;

            if (status == 2)
            {
                DrawWrappedText(g, mFont.tahoma_7b_dark, "Vui lòng hoàn thành nhiệm vụ trước đó để xem", _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
                return;
            }

            string[] steps = GetTaskSteps(taskId);
            for (int i = 0; i < steps.Length; i++)
            {
                int color = 0x888888;
                mFont font = mFont.tahoma_7_grey;
                if (status == 0 || (status == 1 && i < currentStep))
                {
                    color = 0x00B86B;
                    font = mFont.tahoma_7_green2;
                }
                else if (status == 1 && i == currentStep)
                {
                    color = 0x496BFF;
                    font = mFont.tahoma_7_blue;
                }

                string step = steps[i] ?? string.Empty;
                if (status == 1 && i == currentStep && currentTask != null && currentTask.counts != null && i < currentTask.counts.Length && currentTask.counts[i] > 1)
                    step += " (" + currentTask.count + "/" + currentTask.counts[i] + ")";
                DrawBulletPoint(g, bulletX, y + 4, color);
                y = DrawWrappedText(g, font, step, textX, y, maxTextW);
                y += 2;
            }

            string description = GetMainTaskGuide(taskId);
            if (!string.IsNullOrEmpty(description))
            {
                y += 3;
                y = DrawWrappedText(g, mFont.tahoma_7b_dark, description, _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
            }

            y += 5;
            DrawDetailDivider(g, y);
            y += 7;
            mFont.tahoma_7b_red.drawString(g, "Phần thưởng :", _rightBodyRect.X + 4, y, mFont.LEFT);
            y += 16;
            string[] rewards = GetTaskRewards(taskId);
            for (int i = 0; i < rewards.Length; i++)
            {
                mFont.tahoma_7b_red.drawString(g, rewards[i], _rightBodyRect.X + 4, y, mFont.LEFT);
                y += 15;
            }
        }

        private void PaintOtherTaskDetailBody(mGraphics g, int y)
        {
            NpcQuest q = allOtherQuests[_selectedOtherCategoryIndex];
            int midX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            int bulletX = _rightBodyRect.X + 5;
            int textX = bulletX + 10;
            int maxTextW = _rightBodyRect.Width - 20;

            mFont.tahoma_7b_dark.drawString(g, q.title, midX, y, mFont.CENTER);
            y += 18;
            DrawDetailDivider(g, y);
            y += 7;

            if (q.hasQuest)
            {
                int color = q.isComplete ? 0x00B86B : 0x496BFF;
                mFont goalFont = q.isComplete ? mFont.tahoma_7_green2 : mFont.tahoma_7_blue;
                string goal = q.goal ?? string.Empty;
                if (q.maxCount > 0) goal += " (" + q.count + "/" + q.maxCount + ")";
                DrawBulletPoint(g, bulletX, y + 4, color);
                y = DrawWrappedText(g, goalFont, goal, textX, y, maxTextW);
                y += 2;
                DrawBulletPoint(g, bulletX, y + 4, 0x00B86B);
                y = DrawWrappedText(g, mFont.tahoma_7_green2, q.isComplete ? "Quay lại gặp " + q.npcName + " để hoàn tất nhiệm vụ." : q.summary, textX, y, maxTextW);
            }
            else
            {
                y = DrawWrappedText(g, mFont.tahoma_7_grey, "Chưa có nhiệm vụ.", _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
            }

            y += 7;
            DrawDetailDivider(g, y);
            y += 7;
            mFont.tahoma_7b_dark.drawString(g, "Hướng dẫn :", _rightBodyRect.X + 4, y, mFont.LEFT);
            y += 16;
            DrawWrappedText(g, mFont.tahoma_7b_dark, q.defaultHint, _rightBodyRect.X + 4, y, _rightBodyRect.Width - 8);
        }

        private void DrawDetailDivider(mGraphics g, int y)
        {
            g.setColor(0xB89261);
            g.drawLine(_rightBodyRect.X + 3, y, _rightBodyRect.X + _rightBodyRect.Width - 4, y);
        }

        private void PaintLegacyTaskDetailColumn(mGraphics g)
        {
            g.setColor(0xC4B79B);
            g.drawRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);

            using (UiRenderState.Push(g, _rightColRect, clip: true))
            {
                g.setColor(0xDFD2BC);
                g.fillRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);

                int scrollY = (_rightScrollAdapter != null) ? _rightScrollAdapter.ScrollY : 0;
                int curY = _rightColRect.Y + 8 - scrollY;
                int midX = _rightColRect.X + _rightColRect.Width / 2;
                int textX = _rightColRect.X + 18;
                int bulletX = _rightColRect.X + 8;
                int maxBulletW = _rightColRect.Width - 28;

                if (_selectedSubTab == 0)
                {
                    // ---------------- MAIN TASK DETAILS ----------------
                    Task currentTask = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;
                    int currentTaskId = (currentTask != null) ? (int)currentTask.taskId : 0;
                    int currentStepIndex = (currentTask != null) ? currentTask.index : 0;

                    int status;
                    if (_selectedTaskPosition < currentTaskId) status = 0;      // Hoàn thành
                    else if (_selectedTaskPosition == currentTaskId) status = 1; // Đang làm
                    else status = 2;                                          // Khóa

                    string taskTitle = (_selectedTaskPosition >= 0 && _selectedTaskPosition < MainTaskNames.Length)
                        ? MainTaskNames[_selectedTaskPosition]
                        : ("Nhiệm vụ số " + (_selectedTaskPosition + 1));

                    // Title Header
                    mFont.tahoma_7b_dark.drawString(g, taskTitle, midX, curY, mFont.CENTER);
                    curY += 16;

                    // Divider
                    g.setColor(0xD3C4A8);
                    g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                    curY += 8;

                    // Detail description for current task
                    if (_selectedTaskPosition == currentTaskId && currentTask != null && currentTask.details != null && currentTask.details.Length > 0)
                    {
                        string rawDetail = string.Join(" ", currentTask.details);
                        string[] detailLines = mFont.tahoma_7b_dark.splitFontArray(rawDetail, maxBulletW);
                        for (int l = 0; l < detailLines.Length; l++)
                        {
                            mFont.tahoma_7b_dark.drawString(g, detailLines[l], _rightColRect.X + 8, curY, mFont.LEFT);
                            curY += 14;
                        }
                        curY += 6;
                    }

                    // Section: Các bước thực hiện
                    mFont.tahoma_7b_dark.drawString(g, "Các bước thực hiện :", _rightColRect.X + 8, curY, mFont.LEFT);
                    curY += 16;

                    string[] subSteps = (_selectedTaskPosition >= 0 && _selectedTaskPosition < CanonicalTaskSubSteps.Length)
                        ? CanonicalTaskSubSteps[_selectedTaskPosition]
                        : null;

                    if (subSteps != null && subSteps.Length > 0)
                    {
                        for (int s = 0; s < subSteps.Length; s++)
                        {
                            string stepText = subSteps[s];
                            int stepDotColor;
                            mFont stepFont;

                            if (status == 0) // Completed
                            {
                                stepDotColor = 0x27AE60; // Green
                                stepFont = mFont.tahoma_7_green2;
                            }
                            else if (status == 1) // In progress
                            {
                                if (s < currentStepIndex)
                                {
                                    stepDotColor = 0x27AE60; // Green done
                                    stepFont = mFont.tahoma_7_green2;
                                }
                                else if (s == currentStepIndex)
                                {
                                    stepDotColor = 0x2980B9; // Blue active
                                    stepFont = mFont.tahoma_7_blue;
                                    if (currentTask.counts != null && s < currentTask.counts.Length && currentTask.counts[s] > 0)
                                    {
                                        stepText += " (" + currentTask.count + "/" + currentTask.counts[s] + ")";
                                    }
                                }
                                else
                                {
                                    stepDotColor = 0x888888; // Gray upcoming
                                    stepFont = mFont.tahoma_7_grey;
                                }
                            }
                            else // Locked
                            {
                                stepDotColor = 0x888888;
                                stepFont = mFont.tahoma_7_grey;
                            }

                            DrawBulletPoint(g, bulletX, curY + 4, stepDotColor);

                            string[] stepLines = stepFont.splitFontArray(stepText, maxBulletW);
                            for (int sl = 0; sl < stepLines.Length; sl++)
                            {
                                stepFont.drawString(g, stepLines[sl], textX, curY, mFont.LEFT);
                                curY += 14;
                            }
                            curY += 2;
                        }
                    }
                    else
                    {
                        mFont.tahoma_7_grey.drawString(g, "• Chưa có dữ liệu bước làm", textX, curY, mFont.LEFT);
                        curY += 15;
                    }

                    curY += 4;
                    g.setColor(0xD3C4A8);
                    g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                    curY += 8;

                    // Section: Phần thưởng
                    mFont.tahoma_7b_red.drawString(g, "Phần thưởng :", _rightColRect.X + 8, curY, mFont.LEFT);
                    curY += 16;

                    string[] rewards = (_selectedTaskPosition >= 0 && _selectedTaskPosition < CanonicalTaskRewards.Length)
                        ? CanonicalTaskRewards[_selectedTaskPosition]
                        : null;

                    if (rewards != null && rewards.Length > 0)
                    {
                        for (int r = 0; r < rewards.Length; r++)
                        {
                            DrawBulletPoint(g, bulletX, curY + 4, 0xC0392B);
                            mFont.tahoma_7b_red.drawString(g, rewards[r], textX, curY, mFont.LEFT);
                            curY += 15;
                        }
                    }
                    else
                    {
                        DrawBulletPoint(g, bulletX, curY + 4, 0xC0392B);
                        mFont.tahoma_7b_red.drawString(g, "Vàng và Tiềm năng", textX, curY, mFont.LEFT);
                        curY += 15;
                    }
                }
                else
                {
                    // ---------------- OTHER TASK DETAILS ----------------
                    if (_selectedOtherCategoryIndex >= 0 && _selectedOtherCategoryIndex < allOtherQuests.Length)
                    {
                        NpcQuest q = allOtherQuests[_selectedOtherCategoryIndex];

                        // Header: Category Name (NPC)
                        mFont.tahoma_7b_dark.drawString(g, q.categoryName + " (" + q.npcName + ")", midX, curY, mFont.CENTER);
                        curY += 16;

                        // Divider
                        g.setColor(0xD3C4A8);
                        g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                        curY += 8;

                        // Section 1: Mục tiêu
                        mFont.tahoma_7b_dark.drawString(g, "Mục tiêu :", _rightColRect.X + 8, curY, mFont.LEFT);
                        curY += 16;

                        if (q.hasQuest)
                        {
                            int dotColor = q.isComplete ? 0x27AE60 : 0x2980B9;
                            mFont statusFont = q.isComplete ? mFont.tahoma_7_green2 : mFont.tahoma_7_blue;

                            DrawBulletPoint(g, bulletX, curY + 4, dotColor);
                            string goalStr = q.goal;
                            if (q.maxCount > 0 && !goalStr.Contains("(" + q.count + "/"))
                            {
                                goalStr += " (" + q.count + "/" + q.maxCount + ")";
                            }
                            string[] goalLines = statusFont.splitFontArray(goalStr, maxBulletW);
                            for (int l = 0; l < goalLines.Length; l++)
                            {
                                statusFont.drawString(g, goalLines[l], textX, curY, mFont.LEFT);
                                curY += 14;
                            }
                            curY += 2;

                            // Sub-status note
                            DrawBulletPoint(g, bulletX, curY + 4, dotColor);
                            string noteStr = q.isComplete ? "Đã hoàn thành! Hãy quay về gặp " + q.npcName + " để nhận thưởng." : "Đang thực hiện nhiệm vụ.";
                            statusFont.drawString(g, noteStr, textX, curY, mFont.LEFT);
                            curY += 16;
                        }
                        else
                        {
                            DrawBulletPoint(g, bulletX, curY + 4, 0x888888);
                            mFont.tahoma_7_grey.drawString(g, "Chưa nhận nhiệm vụ từ NPC " + q.npcName + ".", textX, curY, mFont.LEFT);
                            curY += 18;
                        }

                        // Divider
                        g.setColor(0xD3C4A8);
                        g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                        curY += 8;

                        // Section 2: Hướng dẫn nhận & làm nhiệm vụ
                        mFont.tahoma_7b_dark.drawString(g, "Hướng dẫn nhận & làm nhiệm vụ :", _rightColRect.X + 8, curY, mFont.LEFT);
                        curY += 16;

                        if (!string.IsNullOrEmpty(q.defaultHint))
                        {
                            string[] hintLines = mFont.tahoma_7b_dark.splitFontArray(q.defaultHint, _rightColRect.Width - 20);
                            for (int h = 0; h < hintLines.Length; h++)
                            {
                                mFont.tahoma_7b_dark.drawString(g, hintLines[h], _rightColRect.X + 10, curY, mFont.LEFT);
                                curY += 14;
                            }
                            curY += 6;
                        }

                        // Divider
                        g.setColor(0xD3C4A8);
                        g.drawLine(_rightColRect.X + 8, curY, _rightColRect.X + _rightColRect.Width - 8, curY);
                        curY += 8;

                        // Section 3: Ghi chú
                        mFont.tahoma_7_grey.drawString(g, "• Tự động đồng bộ với máy chủ khi nhận, hoàn thành hoặc hủy bỏ.", _rightColRect.X + 8, curY, mFont.LEFT);
                    }
                }
            }
        }

    }
}

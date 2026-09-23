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
        public static void SyncMainTask()
        {
            if (_instance == null) return;
            int currentTaskId = GetCurrentTaskId();
            _instance._selectedTaskPosition = GetMainTaskPosition(currentTaskId);
            _instance.ConfigureScrollAdapters();
            _instance._leftScrollAdapter?.ScrollToIndex(_instance._selectedTaskPosition);
        }

        public static void OnNpcDialog(int npcTempId, string text)
        {
            if (npcTempId == IntrinsicNpcId && ModFunc.GI().IsAutoIntrinsicRunning)
            {
                ModFunc.GI().NotifyIntrinsicAutoMenuReady();
                DismissIntrinsicNpcOverlay();
                return;
            }
            if (npcTempId == IntrinsicNpcId && _isOpen && _instance != null && _instance.CaptureIntrinsicDialog(text)) return;
            if (string.IsNullOrEmpty(text)) return;

            // 1. Ngư Dân (Fishing)
            if (npcTempId == 113 || text.Contains("cá cấp") || text.Contains("Cá cấp") || text.Contains("Ngư dân") || text.Contains("Ngư Phủ"))
            {
                if (text.Contains("Mỗi ngày được nhận tối đa") && !text.Contains("Tiến độ:"))
                {
                    questFishing.hasQuest = false;
                    questFishing.isComplete = false;
                    questFishing.count = 0;
                    questFishing.maxCount = 0;
                    questFishing.title = "Nhiệm vụ câu cá";
                    questFishing.goal = "Chưa nhận nhiệm vụ câu cá.";
                }
                else if (text.Contains("Nhiệm vụ") && text.Contains("Tiến độ:"))
                {
                    questFishing.hasQuest = true;
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.StartsWith("Nhiệm vụ"))
                        {
                            questFishing.title = l;
                        }
                        else if (l.Contains("cá cấp") || l.Contains("Cá cấp") || l.StartsWith("Câu"))
                        {
                            questFishing.goal = l;
                        }
                        else if (l.Contains("Tiến độ:"))
                        {
                            string p = l.Replace("Tiến độ:", "").Trim();
                            string[] parts = p.Split('/');
                            if (parts.Length == 2)
                            {
                                int.TryParse(parts[0].Trim(), out questFishing.count);
                                int.TryParse(parts[1].Trim(), out questFishing.maxCount);
                            }
                        }
                        else if (l.Contains("hoàn thành") || l.Contains("nhận thưởng"))
                        {
                            questFishing.isComplete = true;
                        }
                    }
                    if (questFishing.maxCount > 0 && questFishing.count >= questFishing.maxCount)
                    {
                        questFishing.isComplete = true;
                    }
                }
                _instance?.RefreshOtherTaskList();
            }

            // 2. Bò Mộng (Daily Quest)
            if (npcTempId == 47 || npcTempId == 84 || text.Contains("Bò Mộng") || text.Contains("Số nhiệm vụ còn lại trong ngày:"))
            {
                if (text.Contains("Nhiệm vụ hiện tại:"))
                {
                    questBoMong.hasQuest = true;
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.StartsWith("Nhiệm vụ hiện tại:"))
                        {
                            questBoMong.title = l.Replace("Nhiệm vụ hiện tại:", "").Trim();
                            questBoMong.goal = questBoMong.title;
                        }
                        else if (l.Contains("Hiện tại đã hoàn thành:"))
                        {
                            string p = l.Replace("Hiện tại đã hoàn thành:", "").Trim();
                            int slash = p.IndexOf('/');
                            int openParen = p.IndexOf('(');
                            if (slash > 0 && openParen > slash)
                            {
                                string cStr = p.Substring(0, slash).Trim();
                                string mStr = p.Substring(slash + 1, openParen - slash - 1).Trim();
                                int.TryParse(cStr, out questBoMong.count);
                                int.TryParse(mStr, out questBoMong.maxCount);
                            }
                        }
                    }
                    questBoMong.isComplete = (questBoMong.maxCount > 0 && questBoMong.count >= questBoMong.maxCount);
                }
                else if (text.Contains("Tôi có vài nhiệm vụ theo cấp bậc"))
                {
                    questBoMong.hasQuest = false;
                    questBoMong.isComplete = false;
                    questBoMong.count = 0;
                    questBoMong.maxCount = 0;
                    questBoMong.title = "Nhiệm vụ hằng ngày";
                    questBoMong.goal = "Chưa nhận nhiệm vụ hằng ngày từ Bò Mộng.";
                }
                _instance?.RefreshOtherTaskList();
            }

            // 3. Kanao (Vô Hạn Thành)
            if (npcTempId == 112 || text.Contains("Vô Hạn Thành") || text.Contains("Kanao"))
            {
                if (text.Contains("Bạn chưa có nhiệm vụ Vô Hạn Thành"))
                {
                    questKanao.hasQuest = false;
                    questKanao.isComplete = false;
                    questKanao.count = 0;
                    questKanao.maxCount = 0;
                    questKanao.title = "Nhiệm vụ Vô Hạn Thành";
                    questKanao.goal = "Chưa nhận nhiệm vụ Vô Hạn Thành từ Kanao.";
                }
                else if (text.Contains("Nhiệm vụ Vô Hạn Thành") && text.Contains("Tiến độ:"))
                {
                    questKanao.hasQuest = true;
                    questKanao.title = "Nhiệm vụ Vô Hạn Thành";
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string l = lines[i].Trim();
                        if (l.StartsWith("Tiêu diệt"))
                        {
                            questKanao.goal = l;
                        }
                        else if (l.Contains("Tiến độ:"))
                        {
                            string p = l.Replace("Tiến độ:", "").Trim();
                            string[] parts = p.Split('/');
                            if (parts.Length == 2)
                            {
                                int.TryParse(parts[0].Trim(), out questKanao.count);
                                int.TryParse(parts[1].Trim(), out questKanao.maxCount);
                            }
                        }
                    }
                    questKanao.isComplete = (questKanao.maxCount > 0 && questKanao.count >= questKanao.maxCount);
                }
                _instance?.RefreshOtherTaskList();
            }

            // 4. Bang hội (Clan)
            if (text.Contains("Nhiệm vụ hiện tại:") && text.Contains("Đã hạ được"))
            {
                questClan.hasQuest = true;
                string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length; i++)
                {
                    string l = lines[i].Trim();
                    if (l.StartsWith("Nhiệm vụ hiện tại:"))
                    {
                        string payload = l.Replace("Nhiệm vụ hiện tại:", "").Trim();
                        int progressMarker = payload.IndexOf(". Đã hạ được", StringComparison.OrdinalIgnoreCase);
                        if (progressMarker >= 0)
                        {
                            questClan.title = payload.Substring(0, progressMarker).Trim();
                            int.TryParse(payload.Substring(progressMarker + 13).Trim(), out questClan.count);
                        }
                        else questClan.title = payload;
                        questClan.goal = questClan.title;
                        questClan.maxCount = FindFirstPositiveInt(questClan.title);
                        questClan.isComplete = questClan.maxCount > 0 && questClan.count >= questClan.maxCount;
                    }
                }
                _instance?.RefreshOtherTaskList();
            }
        }

        public static void OnThongBao(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            // Fishing events
            if (text.Contains("Đã nhận phần thưởng nhiệm vụ câu cá") || text.Contains("Đã xóa nhiệm vụ mức"))
            {
                questFishing.hasQuest = false;
                questFishing.isComplete = false;
                questFishing.count = 0;
                questFishing.maxCount = 0;
                questFishing.title = "Nhiệm vụ câu cá";
                questFishing.goal = "Chưa nhận nhiệm vụ câu cá.";
                _instance?.RefreshOtherTaskList();
            }
            else if (text.StartsWith("Đã nhận nhiệm vụ"))
            {
                questFishing.hasQuest = true;
                questFishing.isComplete = false;
                questFishing.count = 0;
                int colon = text.IndexOf(':');
                if (colon > 0)
                {
                    questFishing.title = text.Substring(0, colon).Trim();
                    questFishing.goal = text.Substring(colon + 1).Trim().TrimEnd('.');
                }
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("Nhiệm vụ câu cá") && text.Contains("hoàn thành"))
            {
                questFishing.isComplete = true;
                if (questFishing.maxCount > 0) questFishing.count = questFishing.maxCount;
                _instance?.RefreshOtherTaskList();
            }

            // Clan progress messages start with "Nhiệm vụ:". Keep them out of
            // the daily quest bucket, which uses "Nhiệm vụ <tên> ...".
            if (text.StartsWith("Nhiệm vụ:") && text.Contains("đã hoàn thành:"))
            {
                ApplyProgressMessage(questClan, text, "Nhiệm vụ:");
                _instance?.RefreshOtherTaskList();
                return;
            }

            // BoMong events
            if (text.StartsWith("Bạn nhận được nhiệm vụ:"))
            {
                questBoMong.hasQuest = true;
                questBoMong.isComplete = false;
                questBoMong.count = 0;
                questBoMong.title = text.Replace("Bạn nhận được nhiệm vụ:", "").Trim();
                questBoMong.goal = questBoMong.title;
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("hủy bỏ nhiệm vụ"))
            {
                questBoMong.hasQuest = false;
                questBoMong.isComplete = false;
                questBoMong.count = 0;
                questBoMong.maxCount = 0;
                questBoMong.title = "Nhiệm vụ hằng ngày";
                questBoMong.goal = "Chưa nhận nhiệm vụ hằng ngày từ Bò Mộng.";
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("đã hoàn thành:") && text.Contains("%"))
            {
                ApplyProgressMessage(questBoMong, text, "Nhiệm vụ");
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("quay về Bò Mộng trả nhiệm vụ"))
            {
                questBoMong.isComplete = true;
                if (questBoMong.maxCount > 0) questBoMong.count = questBoMong.maxCount;
                _instance?.RefreshOtherTaskList();
            }

            // Kanao events
            if (text.StartsWith("Nhiệm vụ Kanao:"))
            {
                questKanao.hasQuest = true;
                string p = text.Replace("Nhiệm vụ Kanao:", "").Trim().TrimEnd('.');
                string[] parts = p.Split('/');
                if (parts.Length == 2)
                {
                    int.TryParse(parts[0].Trim(), out questKanao.count);
                    int.TryParse(parts[1].Trim(), out questKanao.maxCount);
                }
                if (questKanao.maxCount > 0 && questKanao.count >= questKanao.maxCount)
                {
                    questKanao.isComplete = true;
                }
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("Đã hoàn thành nhiệm vụ Kanao"))
            {
                questKanao.hasQuest = true;
                questKanao.isComplete = true;
                if (questKanao.maxCount > 0) questKanao.count = questKanao.maxCount;
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("nhận thưởng nhiệm vụ Kanao"))
            {
                questKanao.hasQuest = false;
                questKanao.isComplete = false;
                questKanao.count = 0;
                questKanao.maxCount = 0;
                questKanao.title = "Nhiệm vụ Vô Hạn Thành";
                questKanao.goal = "Chưa nhận nhiệm vụ Vô Hạn Thành từ Kanao.";
                _instance?.RefreshOtherTaskList();
            }

            // Clan events
            if (text.Contains("Đã hủy nhiệm vụ bang"))
            {
                questClan.hasQuest = false;
                questClan.isComplete = false;
                questClan.count = 0;
                questClan.maxCount = 0;
                questClan.title = "Nhiệm vụ bang hội";
                questClan.goal = "Chưa nhận nhiệm vụ bang hội.";
                _instance?.RefreshOtherTaskList();
            }
            else if (text.Contains("Tiếp theo hãy về Bang hội báo cáo"))
            {
                questClan.isComplete = true;
                _instance?.RefreshOtherTaskList();
            }
        }

        public void RefreshOtherTaskList()
        {
            ConfigureScrollAdapters();
        }

        private static void ApplyProgressMessage(NpcQuest quest, string text, string titlePrefix)
        {
            int marker = text.IndexOf("đã hoàn thành:", StringComparison.OrdinalIgnoreCase);
            if (marker <= 0) return;
            quest.hasQuest = true;
            string title = text.Substring(0, marker).Trim().TrimEnd('.');
            if (title.StartsWith(titlePrefix)) title = title.Substring(titlePrefix.Length).TrimStart(':', ' ');
            if (!string.IsNullOrEmpty(title))
            {
                quest.title = title;
                quest.goal = title;
            }
            string progress = text.Substring(marker + 14).Trim();
            int slash = progress.IndexOf('/');
            int end = progress.IndexOf('(');
            if (slash > 0)
            {
                int.TryParse(progress.Substring(0, slash).Trim(), out quest.count);
                string max = end > slash ? progress.Substring(slash + 1, end - slash - 1) : progress.Substring(slash + 1);
                int.TryParse(max.Trim(), out quest.maxCount);
            }
            quest.isComplete = quest.maxCount > 0 && quest.count >= quest.maxCount;
        }

        private static int FindFirstPositiveInt(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            int value = 0;
            bool reading = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c >= '0' && c <= '9')
                {
                    reading = true;
                    value = value * 10 + c - '0';
                }
                else if (reading) return value;
            }
            return value;
        }

        private static int GetCurrentTaskId()
        {
            Task task = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            return task != null ? (int)task.taskId : 0;
        }

        private static int GetMainTaskPosition(int taskId)
        {
            if (taskId <= 3) return System.Math.Max(0, taskId);
            if (taskId <= 6) return 4;
            return taskId - 2;
        }

        private static int[] BuildMainTaskSequence()
        {
            int gender = Char.myCharz() != null ? Char.myCharz().cgender : 0;
            if (gender < 0 || gender > 2) gender = 0;
            int maxTaskId = System.Math.Max(LastKnownMainTaskId, GetCurrentTaskId() + 1);
            if (maxTaskId >= CanonicalTaskSubSteps.Length) maxTaskId = CanonicalTaskSubSteps.Length - 1;
            if (maxTaskId < 7) maxTaskId = 7;

            int[] result = new int[maxTaskId - 1];
            result[0] = 0;
            result[1] = 1;
            result[2] = 2;
            result[3] = 3;
            result[4] = 4 + gender;
            int position = 5;
            for (int taskId = 7; taskId <= maxTaskId; taskId++) result[position++] = taskId;
            return result;
        }

        private int GetSelectedTaskId()
        {
            int[] sequence = BuildMainTaskSequence();
            if (_selectedTaskPosition < 0) _selectedTaskPosition = 0;
            if (_selectedTaskPosition >= sequence.Length) _selectedTaskPosition = sequence.Length - 1;
            return sequence[_selectedTaskPosition];
        }

        private void SelectFirstAvailableOtherQuest()
        {
            if (_selectedOtherCategoryIndex >= 0 && _selectedOtherCategoryIndex < allOtherQuests.Length && allOtherQuests[_selectedOtherCategoryIndex].hasQuest) return;
            int[] order = new int[] { 0, 1, 3, 2 };
            for (int i = 0; i < order.Length; i++)
            {
                if (allOtherQuests[order[i]].hasQuest)
                {
                    _selectedOtherCategoryIndex = order[i];
                    return;
                }
            }
            _selectedOtherCategoryIndex = 0;
        }

        private int GetMainTaskRowCount()
        {
            return BuildMainTaskSequence().Length;
        }

        private UiRect GetLeftListViewport()
        {
            return _leftColRect;
        }

        private int GetOtherListHeight()
        {
            int h = 0;
            h += 14 + ((questBoMong.hasQuest ? 1 : 0) + (questKanao.hasQuest ? 1 : 0)) * (OtherTaskRowHeight + 1);
            if (!questBoMong.hasQuest && !questKanao.hasQuest) h += 21;
            h += 4 + 14 + (questClan.hasQuest ? OtherTaskRowHeight + 1 : 21);
            h += 4 + 14 + (questFishing.hasQuest ? OtherTaskRowHeight + 1 : 21);
            return h + 2;
        }

        private int GetRightContentHeight()
        {
            if (_selectedMainTab != 0) return 100;

            if (_selectedSubTab == 0)
            {
                int taskId = GetSelectedTaskId();
                int currentTaskPosition = GetMainTaskPosition(GetCurrentTaskId());
                if (_selectedTaskPosition > currentTaskPosition) return 62;
                int h = 27;
                string[] steps = GetTaskSteps(taskId);
                for (int i = 0; i < steps.Length; i++) h += mFont.tahoma_7b_dark.splitFontArray(steps[i], _rightBodyRect.Width - 30).Length * 14 + 2;
                string description = GetMainTaskGuide(taskId);
                if (!string.IsNullOrEmpty(description)) h += mFont.tahoma_7b_dark.splitFontArray(description, _rightBodyRect.Width - 18).Length * 14 + 7;
                string[] rewards = GetTaskRewards(taskId);
                if (rewards != null) h += 24 + rewards.Length * 15;
                return h + 12;
            }
            else
            {
                NpcQuest q = allOtherQuests[_selectedOtherCategoryIndex];
                int h = 38;
                if (q.hasQuest)
                {
                    h += mFont.tahoma_7b_dark.splitFontArray(q.goal ?? string.Empty, _rightBodyRect.Width - 30).Length * 14 + 2;
                    h += mFont.tahoma_7b_dark.splitFontArray(q.summary ?? string.Empty, _rightBodyRect.Width - 30).Length * 14 + 7;
                }
                else h += 18;
                h += 25 + mFont.tahoma_7b_dark.splitFontArray(q.defaultHint ?? string.Empty, _rightBodyRect.Width - 18).Length * 14;
                return h + 8;
            }
        }

        private static string GetMainTaskName(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.names != null && currentTask.names.Length > 0)
            {
                string liveName = string.Join(" ", currentTask.names).Trim();
                if (!string.IsNullOrEmpty(liveName)) return liveName;
            }
            if (taskId >= 0 && taskId < MainTaskNames.Length) return MainTaskNames[taskId];
            return "Nhiệm vụ";
        }

        private static string[] GetTaskSteps(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.subNames != null && currentTask.subNames.Length > 0)
                return currentTask.subNames;
            if (taskId >= 0 && taskId < CanonicalTaskSubSteps.Length && CanonicalTaskSubSteps[taskId] != null)
                return CanonicalTaskSubSteps[taskId];
            return new string[0];
        }

        private static string GetMainTaskGuide(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.details != null)
            {
                string result = string.Empty;
                for (int i = 0; i < currentTask.details.Length; i++)
                {
                    string line = currentTask.details[i];
                    if (string.IsNullOrEmpty(line) || line.IndexOf("thưởng", StringComparison.OrdinalIgnoreCase) >= 0) break;
                    if (result.Length > 0) result += " ";
                    result += line.Trim();
                }
                if (!string.IsNullOrEmpty(result) && !string.Equals(result, "Chi tiết nhiệm vụ", StringComparison.OrdinalIgnoreCase)) return result;
            }
            if (taskId >= 0 && taskId < CanonicalTaskGuides.Length) return CanonicalTaskGuides[taskId];
            return string.Empty;
        }

        private static string[] GetTaskRewards(int taskId)
        {
            Task currentTask = Char.myCharz() != null ? Char.myCharz().taskMaint : null;
            if (currentTask != null && (int)currentTask.taskId == taskId && currentTask.details != null)
            {
                List<string> rewards = new List<string>();
                for (int i = 0; i < currentTask.details.Length; i++)
                {
                    string line = currentTask.details[i] ?? string.Empty;
                    int marker = line.IndexOf("thưởng", StringComparison.OrdinalIgnoreCase);
                    if (marker < 0) continue;
                    string reward = line.Substring(marker + 6).Trim().TrimStart('-', ':', ' ');
                    if (!string.IsNullOrEmpty(reward)) rewards.Add(reward);
                }
                if (rewards.Count > 0) return rewards.ToArray();
            }
            if (taskId >= 0 && taskId < CanonicalTaskRewards.Length && CanonicalTaskRewards[taskId] != null)
                return CanonicalTaskRewards[taskId];
            return new string[] { "Vàng và tiềm năng" };
        }

    }
}

using System;
using Game2.Assets.src.g;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private const int FriendModeFriends = 0;
        private const int FriendModeInbox = 1;
        private const int FriendModeSearch = 2;
        private const int FriendFocusList = 0;
        private const int FriendFocusChat = 1;
        private const int FriendFocusSearch = 2;
        private const int FriendFocusComposer = 3;
        private const int FriendRowHeight = CompactListRowHeight;
        private const int FriendMessageRowHeight = 38;
        private string _friendSearchQuery = string.Empty;
        private bool _friendChatAtBottom = true;
        private static Image _friendRemoveIcon;
        private static Image _friendAddIcon;
        private static Image _friendAcceptIcon;
        private static Image _friendLocationIcon;

        private void ConfigureFriendRects()
        {
            _friendHeaderRect = new UiRect(_leftColRect.X, _rightColRect.Y, _leftColRect.Width, 22);
            _friendChatHeaderRect = new UiRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, 22);
            int toolbarY = _friendHeaderRect.Bottom + 4;
            int mailWidth = System.Math.Min(44, System.Math.Max(34, (_leftColRect.Width - 6) / 4));
            _friendMailRect = new UiRect(_leftColRect.X + 2, toolbarY, mailWidth, 24);
            _friendFindRect = new UiRect(_leftColRect.Right - 32, toolbarY, 30, 24);
            _friendSearchRect = new UiRect(_friendMailRect.Right + 3, toolbarY,
                System.Math.Max(32, _friendFindRect.X - _friendMailRect.Right - 6), 24);
            _friendHeadingRect = new UiRect(_leftColRect.X + 2, toolbarY + 27,
                _leftColRect.Width - 4, 17);
            _friendListRect = new UiRect(_leftColRect.X + 2, _friendHeadingRect.Bottom,
                _leftColRect.Width - 4, System.Math.Max(1, _leftColRect.Bottom - _friendHeadingRect.Bottom));
            _friendComposerRect = new UiRect(_rightColRect.X, _rightColRect.Bottom - 28,
                _rightColRect.Width, 28);
            _friendChatRect = new UiRect(_rightColRect.X + 2, _friendChatHeaderRect.Bottom + 3,
                _rightColRect.Width - 4,
                System.Math.Max(1, _friendComposerRect.Y - _friendChatHeaderRect.Bottom - 5));
            _friendLocationRect = new UiRect(_friendComposerRect.X + 2,
                _friendComposerRect.Y + 2, 28, 24);
            _friendSendRect = new UiRect(_friendComposerRect.Right - 31,
                _friendComposerRect.Y + 2, 29, 24);

            if (_friendSearchAdapter == null)
            {
                _friendSearchField = new TField();
                _friendSearchAdapter = new TextFieldAdapter(_friendSearchField);
            }
            _friendSearchAdapter.Configure(_friendSearchRect.X + 2, _friendSearchRect.Y + 2,
                _friendSearchRect.Width - 4, _friendSearchRect.Height - 4,
                UiInputType.Any, 32, "Nhập tên ...");
            if (_friendChatAdapter == null)
            {
                _friendChatField = new TField();
                _friendChatAdapter = new TextFieldAdapter(_friendChatField);
            }
            _friendChatAdapter.Configure(_friendLocationRect.Right + 3,
                _friendComposerRect.Y + 2,
                System.Math.Max(30, _friendSendRect.X - _friendLocationRect.Right - 6), 24,
                UiInputType.Any, 80, "Soạn tin nhắn ...");
        }

        private void EnterFriendTab()
        {
            _friendMode = FriendModeFriends;
            _friendFocusArea = FriendFocusList;
            _selectedFriendRow = 0;
            _friendSearchQuery = string.Empty;
            _friendSearchAdapter?.SetText(string.Empty);
            _friendSearchAdapter?.SetFocused(false);
            _friendChatAdapter?.SetFocused(false);
            _friendListRequestUtcTicks = DateTime.UtcNow.Ticks;
            Service.gI().friend(0, -1);
            int activeId = FriendSocialState.gI().ActiveChatFriendId;
            if (activeId <= 0) activeId = _friendChatFieldFriendId;
            if (activeId > 0)
            {
                _friendChatFieldFriendId = -1;
                OpenFriendConversation(activeId);
            }
            else SelectInitialFriendConversation();
        }

        private void LeaveFriendTab()
        {
            BlurFriendInputs();
            int friendId = _friendChatFieldFriendId;
            if (friendId > 0)
                FriendSocialState.gI().DeactivateConversation(friendId);
        }

        private void SwitchToFriendTab()
        {
            _selectedMainTab = 6;
            _keyboardFocus = KeyboardFocusContent;
            _mainTabScrollAdapter.ScrollToIndex(6);
            _leftScrollAdapter?.Reset();
            _rightScrollAdapter?.Reset();
            EnterFriendTab();
            ConfigureScrollAdapters();
        }

        private int GetFriendRowCount()
        {
            FriendSocialState state = FriendSocialState.gI();
            if (_friendMode == FriendModeSearch) return state.Search.Results.Count;
            if (_friendMode == FriendModeInbox) return state.Inbox.Results.Count;
            int count = GameCanvas.panel != null ? GameCanvas.panel.vFriend.size() : 0;
            for (int i = 0; i < state.OutgoingRequests.Count; i++)
            {
                FriendOutgoingRequest request = state.OutgoingRequests[i];
                if (request != null && FindFriendInfo(request.PlayerId) == null) count++;
            }
            return count;
        }

        private int GetFriendMessageCount()
        {
            FriendConversation conversation;
            int id = FriendSocialState.gI().ActiveChatFriendId;
            return id > 0 && FriendSocialState.gI().Conversations.TryPeek(id, out conversation)
                ? conversation.Messages.Count : 0;
        }

        private InfoItem FindFriendInfo(int friendId)
        {
            if (GameCanvas.panel == null) return null;
            MyVector list = GameCanvas.panel.vFriend;
            for (int i = 0; i < list.size(); i++)
            {
                InfoItem info = list.elementAt(i) as InfoItem;
                if (info != null && info.charInfo != null && info.charInfo.charID == friendId)
                    return info;
            }
            return null;
        }

        private FriendOutgoingRequest GetOutgoingFriendRow(int index)
        {
            int baseCount = GameCanvas.panel != null ? GameCanvas.panel.vFriend.size() : 0;
            int target = index - baseCount;
            if (target < 0) return null;
            FriendSocialState state = FriendSocialState.gI();
            for (int i = 0; i < state.OutgoingRequests.Count; i++)
            {
                FriendOutgoingRequest request = state.OutgoingRequests[i];
                if (request == null || FindFriendInfo(request.PlayerId) != null) continue;
                if (target-- == 0) return request;
            }
            return null;
        }

        private void SelectInitialFriendConversation()
        {
            FriendSocialState state = FriendSocialState.gI();
            if (state.ActiveChatFriendId > 0 || GameCanvas.panel == null) return;
            MyVector list = GameCanvas.panel.vFriend;
            for (int i = 0; i < list.size(); i++)
            {
                InfoItem info = list.elementAt(i) as InfoItem;
                if (info == null || info.charInfo == null) continue;
                bool online;
                if (!state.TryGetPresence(info.charInfo.charID, out online)) online = info.isOnline;
                if (!online) continue;
                _selectedFriendRow = i;
                OpenFriendConversation(info.charInfo.charID);
                break;
            }
        }

        private void OpenFriendConversation(int friendId)
        {
            if (friendId <= 0) return;
            SaveFriendChatDraft();
            FriendSocialState state = FriendSocialState.gI();
            FriendConversation conversation = state.ActivateConversation(friendId);
            Service.gI().loadFriendSocialV2Profile(friendId);
            _friendChatFieldFriendId = friendId;
            _friendChatAdapter?.SetText(conversation.Draft ?? string.Empty);
            _friendChatAdapter?.SetFocused(false);
            _friendChatAtBottom = true;
            ConfigureScrollAdapters();
            _rightScrollAdapter?.ScrollToIndex(System.Math.Max(0, conversation.Messages.Count - 1));
        }

        private void SaveFriendChatDraft()
        {
            FriendConversation conversation;
            if (_friendChatFieldFriendId > 0 && _friendChatAdapter != null
                && FriendSocialState.gI().Conversations.TryPeek(_friendChatFieldFriendId, out conversation))
                conversation.Draft = _friendChatAdapter.GetText();
        }

        private void SetFriendMode(int mode)
        {
            if (mode == _friendMode) return;
            _friendMode = mode;
            _selectedFriendRow = 0;
            _leftScrollAdapter?.Reset();
            _friendSearchAdapter?.SetFocused(false);
            if (mode == FriendModeInbox)
            {
                FriendSocialState state = FriendSocialState.gI();
                if (state.SupportsSocialV2 && !state.Inbox.IsLoading && state.Inbox.Results.Count == 0)
                    Service.gI().loadFriendSocialV2Inbox(++_friendInboxToken, 0);
            }
            ConfigureScrollAdapters();
        }

        private void SearchFriends()
        {
            string query = (_friendSearchAdapter?.GetText() ?? string.Empty).Trim();
            if (query.Length == 0)
            {
                _friendSearchQuery = string.Empty;
                FriendSocialState.gI().Search.Clear();
                SetFriendMode(FriendModeFriends);
                return;
            }
            if (query.Length < 2)
            {
                GameScr.info1.addInfo("Cần nhập ít nhất 2 ký tự", 0);
                return;
            }
            if (!FriendSocialState.gI().SupportsSocialV2)
            {
                GameScr.info1.addInfo("Máy chủ chưa hỗ trợ tìm bạn", 0);
                return;
            }
            _friendSearchQuery = query;
            SetFriendMode(FriendModeSearch);
            Service.gI().searchFriendSocialV2(++_friendSearchToken, 0, query);
            _friendSearchAdapter?.SetFocused(false);
        }

        private void MaybeLoadFriendNextPage()
        {
            SelectInitialFriendConversation();
            FriendSocialState state = FriendSocialState.gI();
            if (_friendMode == FriendModeInbox && state.SupportsSocialV2
                && state.Inbox.RequestToken < 0 && !state.Inbox.IsLoading)
                Service.gI().loadFriendSocialV2Inbox(++_friendInboxToken, 0);
            if (_leftScrollAdapter == null || _leftScrollAdapter.ScrollLimit <= 0
                || _leftScrollAdapter.ScrollY < _leftScrollAdapter.ScrollLimit - 2) return;
            if (_friendMode == FriendModeSearch && !state.Search.IsLoading && state.Search.HasMore)
                Service.gI().searchFriendSocialV2(_friendSearchToken, state.Search.NextCursor, _friendSearchQuery);
            else if (_friendMode == FriendModeInbox && !state.Inbox.IsLoading && state.Inbox.HasMore)
                Service.gI().loadFriendSocialV2Inbox(_friendInboxToken, state.Inbox.NextCursor);
        }

        private bool IsFriendInputFocused()
        {
            return (_friendSearchAdapter != null && _friendSearchAdapter.IsFocused)
                || (_friendChatAdapter != null && _friendChatAdapter.IsFocused);
        }

        private void BlurFriendInputs()
        {
            SaveFriendChatDraft();
            _friendSearchAdapter?.SetFocused(false);
            _friendChatAdapter?.SetFocused(false);
        }

        private bool HandleFriendTextInput()
        {
            if (!IsFriendInputFocused()) return false;
            TextFieldAdapter field = _friendSearchAdapter.IsFocused ? _friendSearchAdapter : _friendChatAdapter;
            if (GameCanvas.keyPressed[23] || GameCanvas.keyPressed[4])
            {
                GameCanvas.keyPressed[23] = false;
                GameCanvas.keyPressed[4] = false;
                field.KeyPressed(14);
                return true;
            }
            if (GameCanvas.keyPressed[24] || GameCanvas.keyPressed[6])
            {
                GameCanvas.keyPressed[24] = false;
                GameCanvas.keyPressed[6] = false;
                field.KeyPressed(15);
                return true;
            }
            if (Main.isPC && GameCanvas.keyPressed[14])
            {
                GameCanvas.keyPressed[14] = false;
                field.KeyPressed(-8);
                SaveFriendChatDraft();
                return true;
            }
            if (GameCanvas.keyPressed[15] || GameCanvas.keyPressed[25]
                || GameCanvas.keyAsciiPress == 10 || GameCanvas.keyAsciiPress == 13)
            {
                GameCanvas.keyPressed[15] = false;
                GameCanvas.keyPressed[25] = false;
                GameCanvas.keyAsciiPress = 0;
                if (field == _friendSearchAdapter) SearchFriends();
                else SendFriendChat();
                return true;
            }
            if (GameCanvas.keyAsciiPress != 0)
            {
                field.KeyPressed(GameCanvas.keyAsciiPress);
                GameCanvas.keyAsciiPress = 0;
                SaveFriendChatDraft();
                return true;
            }
            return false;
        }

        private void SendFriendChat()
        {
            FriendSocialState state = FriendSocialState.gI();
            int friendId = state.ActiveChatFriendId;
            string message = (_friendChatAdapter?.GetText() ?? string.Empty).Trim();
            if (friendId <= 0 || message.Length == 0) return;
            InfoItem info = FindFriendInfo(friendId);
            bool online;
            if (!state.TryGetPresence(friendId, out online))
                online = info != null && info.isOnline
                    || state.Profile != null && state.Profile.FriendId == friendId && state.Profile.Online;
            if (!online)
            {
                GameScr.info1.addInfo("Bạn hiện offline", 0);
                return;
            }
            if (message.Length > 80) message = message.Substring(0, 80);
            if (!Service.gI().sendFriendSocialV2Chat(message, friendId)) return;
            FriendConversation conversation = state.Conversations.GetOrCreate(friendId);
            conversation.Draft = string.Empty;
            _friendChatAdapter.SetText(string.Empty);
            _friendChatAdapter.SetFocused(false);
            _friendChatAtBottom = true;
        }

        private bool HandleFriendPointerInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                if (GameCanvas.isPointer(_friendMailRect.X, _friendMailRect.Y,
                    _friendMailRect.Width, _friendMailRect.Height))
                {
                    SetFriendMode(_friendMode == FriendModeInbox ? FriendModeFriends : FriendModeInbox);
                    GameCanvas.clearAllPointerEvent();
                    return true;
                }
                if (GameCanvas.isPointer(_friendFindRect.X, _friendFindRect.Y,
                    _friendFindRect.Width, _friendFindRect.Height))
                {
                    SearchFriends();
                    GameCanvas.clearAllPointerEvent();
                    return true;
                }
                if (GameCanvas.isPointer(_friendSearchRect.X, _friendSearchRect.Y,
                    _friendSearchRect.Width, _friendSearchRect.Height))
                {
                    _friendFocusArea = FriendFocusSearch;
                    _friendSearchAdapter.SetFocused(true);
                    GameCanvas.clearAllPointerEvent();
                    return true;
                }
                if (GameCanvas.isPointer(_friendLocationRect.X, _friendLocationRect.Y,
                    _friendLocationRect.Width, _friendLocationRect.Height))
                {
                    int id = FriendSocialState.gI().ActiveChatFriendId;
                    if (id > 0) Service.gI().shareFriendSocialV2Location(id);
                    GameCanvas.clearAllPointerEvent();
                    return true;
                }
                if (GameCanvas.isPointer(_friendSendRect.X, _friendSendRect.Y,
                    _friendSendRect.Width, _friendSendRect.Height))
                {
                    SendFriendChat();
                    GameCanvas.clearAllPointerEvent();
                    return true;
                }
                if (GameCanvas.isPointer(_friendChatField.x, _friendChatField.y,
                    _friendChatField.width, _friendChatField.height))
                {
                    _friendFocusArea = FriendFocusComposer;
                    _friendChatAdapter.SetFocused(true);
                    GameCanvas.clearAllPointerEvent();
                    return true;
                }
            }
            if (_leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int row))
            {
                if (row >= 0)
                {
                    _selectedFriendRow = row;
                    _friendFocusArea = FriendFocusList;
                    ActivateFriendRow(row, GameCanvas.px);
                }
                return true;
            }
            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int messageRow))
            {
                _friendFocusArea = FriendFocusChat;
                if (messageRow >= 0) _selectedFriendMessage = messageRow;
                _friendChatAtBottom = _rightScrollAdapter.ScrollY >= _rightScrollAdapter.ScrollLimit - 2;
                return true;
            }
            return false;
        }

        private void ActivateFriendRow(int row, int pointerX)
        {
            FriendSocialState state = FriendSocialState.gI();
            if (_friendMode == FriendModeFriends)
            {
                int friendCount = GameCanvas.panel != null ? GameCanvas.panel.vFriend.size() : 0;
                if (row >= friendCount) return;
                InfoItem info = GameCanvas.panel.vFriend.elementAt(row) as InfoItem;
                if (info == null || info.charInfo == null) return;
                int friendId = info.charInfo.charID;
                if (pointerX >= _friendListRect.Right - 25)
                {
                    Service.gI().friend(2, friendId);
                    return;
                }
                bool online;
                if (!state.TryGetPresence(friendId, out online)) online = info.isOnline;
                if (online) OpenFriendConversation(friendId);
                else GameScr.info1.addInfo("Bạn hiện offline", 0);
                return;
            }
            if (_friendMode == FriendModeSearch)
            {
                if (row >= state.Search.Results.Count) return;
                FriendSearchResult result = state.Search.Results[row];
                if (result.Relationship == 2 && Service.gI().sendFriendSocialV2Request(result.PlayerId))
                    result.Relationship = 1;
                else if (result.Relationship == 0) OpenFriendConversation(result.PlayerId);
                return;
            }
            if (row >= state.Inbox.Results.Count || state.IsInboxOperationPending) return;
            FriendInboxRequest request = state.Inbox.Results[row];
            if (pointerX >= _friendListRect.Right - 24)
                Service.gI().acceptFriendSocialV2Request(request.RequestId);
            else if (pointerX >= _friendListRect.Right - 46)
                Service.gI().rejectFriendSocialV2Request(request.RequestId);
        }

        private void MoveFriendHorizontalFocus(int direction)
        {
            if (direction < 0)
            {
                if (_friendFocusArea == FriendFocusList || _friendFocusArea == FriendFocusSearch)
                    _keyboardFocus = KeyboardFocusMainTabs;
                else _friendFocusArea = FriendFocusList;
            }
            else _friendFocusArea = FriendFocusChat;
            SoundMn.gI().panelClick();
        }

        private void MoveFriendVerticalSelection(int direction)
        {
            if (_friendFocusArea == FriendFocusChat)
            {
                int count = GetFriendMessageCount();
                if (count > 0)
                {
                    _selectedFriendMessage = System.Math.Max(0,
                        System.Math.Min(count - 1, _selectedFriendMessage + direction));
                    _rightScrollAdapter?.ScrollToIndex(_selectedFriendMessage);
                }
                return;
            }
            if (_friendFocusArea == FriendFocusSearch)
            {
                if (direction > 0) _friendFocusArea = FriendFocusList;
                return;
            }
            int rows = GetFriendRowCount();
            if (direction < 0 && _selectedFriendRow == 0)
            {
                _friendFocusArea = FriendFocusSearch;
                return;
            }
            if (rows > 0)
            {
                _selectedFriendRow = System.Math.Max(0,
                    System.Math.Min(rows - 1, _selectedFriendRow + direction));
                _leftScrollAdapter?.ScrollToIndex(_selectedFriendRow);
            }
        }

        private void HandleFriendConfirm()
        {
            if (_friendFocusArea == FriendFocusSearch)
            {
                _friendSearchAdapter?.SetFocused(true);
                return;
            }
            if (_friendFocusArea == FriendFocusComposer)
            {
                SendFriendChat();
                return;
            }
            if (_friendFocusArea == FriendFocusChat)
            {
                _friendFocusArea = FriendFocusComposer;
                _friendChatAdapter?.SetFocused(true);
                return;
            }
            ActivateFriendRow(_selectedFriendRow,
                _friendMode == FriendModeInbox ? _friendListRect.Right - 8 : _friendListRect.X + 8);
        }
    }
}

using System;
using Game1.Assets.src.g;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Nro.UI;

namespace Game1.UI.CustomMenu
{
    public partial class CustomMenuScr
    {
        private void PaintFriendTabContent(mGraphics g)
        {
            UiMenuTheme.PaintHeader(g, _friendHeaderRect, "Bạn bè");
            UiMenuTheme.PaintHeader(g, _friendChatHeaderRect, "Chat");
            UiMenuTheme.PaintSurface(g, _friendListRect, 0xDFCFB9);
            UiMenuTheme.PaintSurface(g, _friendChatRect, 0xDFCFB9);
            PaintFriendToolbar(g);
            PaintFriendHeading(g);

            if (GetFriendRowCount() == 0)
            {
                string empty = _friendMode == FriendModeInbox
                    ? (FriendSocialState.gI().Inbox.IsLoading ? "Đang tải..." : "Không có lời mời kết bạn")
                    : _friendMode == FriendModeSearch
                        ? (FriendSocialState.gI().Search.IsLoading ? "Đang tìm..." : "Không tìm thấy bạn")
                        : "Chưa có bạn bè";
                mFont.tahoma_7_grey.drawString(g, empty,
                    _friendListRect.X + _friendListRect.Width / 2,
                    _friendListRect.Y + 11, mFont.CENTER);
            }
            _leftScrollAdapter?.Paint(g, PaintFriendRow);
            _leftScrollAdapter?.PaintScrollbar(g, _friendListRect);

            PaintFriendChat(g);
            _rightScrollAdapter?.PaintScrollbar(g, _friendChatRect);
            PaintFriendComposer(g);
        }

        private void PaintFriendToolbar(mGraphics g)
        {
            UiMenuTheme.PaintButton(g, _friendMailRect, "Thư", _friendMode == FriendModeInbox,
                _keyboardFocus == KeyboardFocusContent && _friendFocusArea == FriendFocusSearch);
            UiMenuTheme.PaintCard(g, _friendSearchRect, 0xFFFDF9, 0xC7AF90);
            _friendSearchAdapter?.Paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            UiMenuTheme.PaintButton(g, _friendFindRect, "Tìm", _friendMode == FriendModeSearch);
        }

        private void PaintFriendHeading(mGraphics g)
        {
            string title = _friendMode == FriendModeInbox ? "----- Hộp thư -----"
                : _friendMode == FriendModeSearch ? "----- Kết quả tìm kiếm -----"
                : "Danh sách bạn bè";
            mFont.tahoma_7b_dark.drawString(g, title,
                _friendHeadingRect.X + _friendHeadingRect.Width / 2,
                _friendHeadingRect.Y + 4, mFont.CENTER);
        }

        private void PaintFriendRow(mGraphics g, int row, UiRect bounds)
        {
            FriendSocialState state = FriendSocialState.gI();
            string name;
            string status;
            int head;
            Char character = null;
            int friendId = -1;
            int iconKind = 0;
            bool green = false;

            if (_friendMode == FriendModeFriends)
            {
                int friendCount = GameCanvas.panel != null ? GameCanvas.panel.vFriend.size() : 0;
                if (row < friendCount)
                {
                    InfoItem info = GameCanvas.panel.vFriend.elementAt(row) as InfoItem;
                    if (info == null || info.charInfo == null) return;
                    character = info.charInfo;
                    friendId = character.charID;
                    name = character.cName + " (" + friendId + ")";
                    head = character.head;
                    bool online;
                    if (!state.TryGetPresence(friendId, out online)) online = info.isOnline;
                    status = online ? "online" : "offline";
                    FriendConversation conversation;
                    if (state.Conversations.TryPeek(friendId, out conversation)
                        && conversation.UnreadCount > 0) status += " - Tin nhắn mới";
                    green = online;
                    iconKind = 1;
                }
                else
                {
                    FriendOutgoingRequest request = GetOutgoingFriendRow(row);
                    if (request == null) return;
                    name = request.Name + " (" + request.PlayerId + ")";
                    head = request.Head;
                    status = "Chờ đồng ý";
                }
            }
            else if (_friendMode == FriendModeSearch)
            {
                if (row >= state.Search.Results.Count) return;
                FriendSearchResult result = state.Search.Results[row];
                name = result.Name + " (" + result.PlayerId + ")";
                head = result.Head;
                status = result.Relationship == 2 ? "Có thể kết bạn"
                    : result.Relationship == 1 ? "Chờ đồng ý" : "Đã là bạn bè";
                green = result.Relationship == 0;
                iconKind = result.Relationship == 2 ? 2 : 0;
            }
            else
            {
                if (row >= state.Inbox.Results.Count) return;
                FriendInboxRequest request = state.Inbox.Results[row];
                name = request.SenderName + " (" + request.SenderId + ")";
                head = request.Head;
                status = state.IsInboxOperationPending && state.PendingInboxRequestId == request.RequestId
                    ? "Đang xử lý..." : "Lời mời kết bạn";
                green = true;
                iconKind = state.IsInboxOperationPending ? 0 : 3;
            }

            UiRect card = new UiRect(bounds.X + 1, bounds.Y + 1,
                bounds.Width - 3, bounds.Height - 2);
            bool active = _friendMode == FriendModeFriends && friendId > 0
                && state.ActiveChatFriendId == friendId;
            UiMenuTheme.PaintCard(g, card, active ? 0xFFF20B : 0xF9F7F3, 0xC7B39B);
            UiRect avatar = new UiRect(card.X + 1, card.Y + 1, 28, card.Height - 2);
            PaintFriendAvatar(g, character, head, avatar);
            int textX = avatar.Right + 4;
            int reserve = iconKind == 3 ? 42 : iconKind != 0 ? 20 : 4;
            int textWidth = System.Math.Max(20, card.Right - reserve - textX);
            mFont.tahoma_7b_dark.drawString(g,
                TruncateString(mFont.tahoma_7b_dark, name, textWidth),
                textX, card.Y + 3, mFont.LEFT);
            (green ? mFont.tahoma_7b_green : mFont.tahoma_7_grey).drawString(g,
                TruncateString(green ? mFont.tahoma_7b_green : mFont.tahoma_7_grey,
                    status, textWidth), textX, card.Y + 16, mFont.LEFT);
            if (iconKind == 1) DrawFriendActionIcon(g,
                ref _friendRemoveIcon, "/mainimage/social_remove.png", card.Right - 11, card.Y + card.Height / 2);
            else if (iconKind == 2) DrawFriendActionIcon(g,
                ref _friendAddIcon, "/mainimage/social_add.png", card.Right - 11, card.Y + card.Height / 2);
            else if (iconKind == 3)
            {
                DrawFriendActionIcon(g, ref _friendRemoveIcon,
                    "/mainimage/social_remove.png", card.Right - 32, card.Y + card.Height / 2);
                DrawFriendActionIcon(g, ref _friendAcceptIcon,
                    "/mainimage/social_accept.png", card.Right - 11, card.Y + card.Height / 2);
            }
        }

        private void PaintFriendChat(mGraphics g)
        {
            int friendId = FriendSocialState.gI().ActiveChatFriendId;
            if (friendId <= 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chọn bạn để trò chuyện",
                    _friendChatRect.X + _friendChatRect.Width / 2,
                    _friendChatRect.Y + 12, mFont.CENTER);
                return;
            }
            if (GetFriendMessageCount() == 0)
                mFont.tahoma_7_grey.drawString(g, "Chưa có tin nhắn",
                    _friendChatRect.X + _friendChatRect.Width / 2,
                    _friendChatRect.Y + 12, mFont.CENTER);
            _rightScrollAdapter?.Paint(g, PaintFriendChatMessage);
        }

        private void PaintFriendChatMessage(mGraphics g, int index, UiRect bounds)
        {
            FriendSocialState state = FriendSocialState.gI();
            int friendId = state.ActiveChatFriendId;
            FriendConversation conversation;
            if (friendId <= 0 || !state.Conversations.TryPeek(friendId, out conversation)
                || index >= conversation.Messages.Count) return;
            FriendChatMessage message = conversation.Messages[index];
            bool fromFriend = message.SenderId == friendId;
            UiRect card = new UiRect(bounds.X + 1, bounds.Y + 1,
                bounds.Width - 3, bounds.Height - 2);
            UiMenuTheme.PaintCard(g, card, 0xFFFEFC, 0xCEBDA8);
            const int avatarSize = 27;
            UiRect avatar = new UiRect(fromFriend ? card.X + 2 : card.Right - avatarSize - 2,
                card.Y + 4, avatarSize, avatarSize);
            InfoItem info = fromFriend ? FindFriendInfo(friendId) : null;
            Char character = fromFriend ? info != null ? info.charInfo : null : Char.myCharz();
            int head = character != null ? character.head
                : state.Profile != null && state.Profile.FriendId == friendId ? state.Profile.Head : -1;
            PaintFriendAvatar(g, character, head, avatar);
            int textX = fromFriend ? avatar.Right + 4 : card.X + 4;
            int textWidth = System.Math.Max(12, card.Width - avatarSize - 12);
            string sender = fromFriend ? GetFriendDisplayName(friendId) : "Bạn";
            string age = FormatFriendMessageAge(message.ReceivedAtUtcTicks);
            int ageWidth = mFont.tahoma_7_grey.getWidth(age);
            mFont.tahoma_7b_blue.drawString(g,
                TruncateString(mFont.tahoma_7b_blue, sender,
                    System.Math.Max(12, textWidth - ageWidth - 3)),
                textX, card.Y + 2, mFont.LEFT);
            mFont.tahoma_7_grey.drawString(g, age,
                card.Right - (fromFriend ? 4 : avatarSize + 5), card.Y + 2, mFont.RIGHT);
            string body = message.Kind == FriendChatMessageKind.Location && message.Location != null
                ? "Vị trí: bản đồ " + message.Location.MapId + ", khu " + message.Location.ZoneId
                : message.Text ?? string.Empty;
            string[] lines = mFont.tahoma_7b_dark.splitFontArray(body, textWidth);
            for (int i = 0; i < lines.Length && i < 2; i++)
                mFont.tahoma_7b_dark.drawString(g,
                    i == 1 && lines.Length > 2
                        ? TruncateString(mFont.tahoma_7b_dark, lines[i] + "...", textWidth)
                        : lines[i], textX, card.Y + 14 + i * 10, mFont.LEFT);
        }

        private string GetFriendDisplayName(int friendId)
        {
            InfoItem info = FindFriendInfo(friendId);
            if (info != null && info.charInfo != null) return info.charInfo.cName ?? "Bạn bè";
            FriendSocialProfile profile = FriendSocialState.gI().Profile;
            return profile != null && profile.FriendId == friendId ? profile.Name : "Bạn bè";
        }

        private static string FormatFriendMessageAge(long receivedAtUtcTicks)
        {
            if (receivedAtUtcTicks <= 0) return "vừa xong";
            long minutes = (DateTime.UtcNow.Ticks - receivedAtUtcTicks) / TimeSpan.TicksPerMinute;
            if (minutes < 1) return "vừa xong";
            if (minutes < 60) return minutes + " phút trước";
            long hours = minutes / 60;
            return hours < 24 ? hours + " giờ trước" : (hours / 24) + " ngày trước";
        }

        private void PaintFriendComposer(mGraphics g)
        {
            UiMenuTheme.PaintSurface(g, _friendComposerRect, 0xEDE2D3);
            UiMenuTheme.PaintButton(g, _friendLocationRect, string.Empty, false);
            if (_friendLocationIcon == null)
                _friendLocationIcon = GameCanvas.loadImage("/mainimage/social_location.png");
            if (_friendLocationIcon != null)
                g.drawImageScale(_friendLocationIcon, _friendLocationRect.X + 5,
                    _friendLocationRect.Y + 3, 18, 18, mGraphics.TRANS_NONE);
            UiMenuTheme.PaintCard(g,
                new UiRect(_friendChatField.x - 2, _friendChatField.y,
                    _friendChatField.width + 4, _friendChatField.height), 0xFFFDF9, 0xC7AF90);
            _friendChatAdapter?.Paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            UiMenuTheme.PaintButton(g, _friendSendRect, "Gửi", false,
                _keyboardFocus == KeyboardFocusContent && _friendFocusArea == FriendFocusComposer);
        }

        private static void DrawFriendActionIcon(mGraphics g, ref Image image,
            string path, int centerX, int centerY)
        {
            if (image == null) image = GameCanvas.loadImage(path);
            if (image != null)
                g.drawImageScale(image, centerX - 9, centerY - 9,
                    18, 18, mGraphics.TRANS_NONE);
        }

        private static void PaintFriendAvatar(mGraphics g, Char character, int fallbackHead, UiRect rect)
        {
            g.setColor(0xB5A188);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height, 4);
            using (UiRenderState.Push(g, rect, clip: true))
            {
                try
                {
                    if (character != null && character.headICON >= 0)
                    {
                        SmallImage.drawSmallImage(g, character.headICON,
                            rect.X + rect.Width / 2, rect.Y + rect.Height / 2,
                            0, StaticObj.VCENTER_HCENTER);
                        return;
                    }
                    int head = character != null ? character.head : fallbackHead;
                    if (head < 0 || GameScr.parts == null || head >= GameScr.parts.Length
                        || GameScr.parts[head] == null) return;
                    Part part = GameScr.parts[head];
                    int frame = Char.CharInfo[0][0][0];
                    if (part.pi == null || frame < 0 || frame >= part.pi.Length
                        || part.pi[frame] == null) return;
                    SmallImage.drawSmallImage(g, part.pi[frame].id,
                        rect.X + rect.Width / 2 + Char.CharInfo[0][0][1] + part.pi[frame].dx - 3,
                        rect.Bottom, 0, mGraphics.LEFT | mGraphics.BOTTOM);
                }
                catch (Exception)
                {
                    // Avatar art is optional while part images are still loading.
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Game1.Assets.src.g;
using Game1.UI.Adapters;
using Game1.UI.Components;
using Nro.UI;

namespace Game1.UI.CustomMenu
{
    public partial class CustomMenuScr : mScreen
    {
        private static CustomMenuScr _instance;
        private static bool _isOpen;
        private static long _friendListRequestUtcTicks;
        private static readonly UiActionButtonStyle ClanMessageOptionButtonStyle = new UiActionButtonStyle
        {
            NormalFill = 0xE99A00,
            Border = 0xF6C13A,
            NormalFont = mFont.tahoma_7b_dark,
            TextHeight = 9
        };
        private mScreen _previousScreen;
        private UiInputContext _inputContext;

        public static bool IsOpen
        {
            get { return _isOpen; }
        }

        public static bool IsFriendTabOpen
        {
            get { return _isOpen && _instance != null && _instance._selectedMainTab == 6; }
        }

        public static bool ConsumeFriendListResponseForCustomMenu()
        {
            bool pending = _friendListRequestUtcTicks > 0
                && DateTime.UtcNow.Ticks - _friendListRequestUtcTicks < TimeSpan.TicksPerSecond * 10;
            _friendListRequestUtcTicks = 0;
            return IsFriendTabOpen || pending;
        }

        // Tabs state
        private int _selectedMainTab = 0; // 0: Nhiệm vụ, 1: Hành trang, 2: Kỹ năng, 3: Bang hội, 4: Chức năng, 5: Đệ tử, 6: Bạn bè
        private int _selectedSubTab = 0;  // 0: Nhiệm vụ chính, 1: Nhiệm vụ khác
        private int _selectedTaskPosition = 0;
        private int _selectedOtherCategoryIndex = 0; // 0: Bò Mộng, 1: Kanao, 2: Ngư Dân, 3: Bang Hội
        private int _selectedInventoryLeftTab;
        private int _selectedInventoryBagTab;
        private readonly UiTabBar _inventoryLeftTabBar = new UiTabBar();
        private readonly UiTabBar _inventoryBagTabBar = new UiTabBar();
        private int _inventoryFocusArea;
        private int _selectedInventoryBodySlot = -1;
        private int _selectedInventoryBagSlot = -1;
        private int _selectedInventoryAction;
        private bool _showInventoryDetail;
        private int _selectedClanView;
        private int _clanFocusArea;
        private int _selectedClanFunction;
        private int _selectedClanRow;
        private int _selectedClanSideAction;
        private int _selectedClanStorageSlot = -1;
        private bool _clanStorageLoaded;
        private bool _clanChatFocused;
        private TField _clanChatField;
        private int _clanDialogMode;
        private int _clanDialogFocus;
        private TField _clanDialogField;
        private int _selectedSkillRow = -1;
        private bool _showSkillKeyPicker;
        private int _skillFocusArea = SkillFocusList;
        private int _selectedPotentialAction = -1;
        private int _selectedIntrinsicAction = -1;
        private int _intrinsicActionCount;
        private string _intrinsicDialogText = string.Empty;
        private readonly string[] _intrinsicActionLabels = new string[4];
        private readonly int[] _intrinsicActionServerIndices = new int[4];
        private bool _waitingIntrinsicMenu;
        private bool _waitingIntrinsicList;
        private bool _showIntrinsicList;
        private bool _showIntrinsicConfirmation;
        private bool _expectingIntrinsicConfirmation;
        private int _selectedIntrinsicListIndex = -1;
        private int _selectedIntrinsicSideAction;
        private bool _showIntrinsicInput;
        private int _intrinsicInputFocus;
        private TField _intrinsicInputField;
        private int _selectedSkillKeyIndex;
        private int _selectedFunction;
        private int _functionView;
        private int _functionFocusArea;
        private int _selectedFunctionRow;
        private bool _functionWorldChatFocused;
        private TField _functionWorldChatField;
        private int _keyboardFocus = KeyboardFocusContent;
        private bool _hasInitializedState;

        // Scroll adapters
        private UiScrollList _leftScrollAdapter;
        private UiScrollList _rightScrollAdapter;
        private readonly UiScrollList _mainTabScrollAdapter = new UiScrollList();

        // Layout bounds
        private UiRect _frameRect;
        private UiRect _tabBarRect;
        private UiRect _footerRect;
        private UiRect _contentRect;

        // Task tab layout bounds
        private UiRect _subTab0Rect;
        private UiRect _subTab1Rect;
        private UiRect _inventoryLeftTab0Rect;
        private UiRect _inventoryLeftTab1Rect;
        private UiRect _inventoryBagTab0Rect;
        private UiRect _inventoryBagTab1Rect;
        private UiRect _leftColRect;
        private UiRect _rightColRect;
        private UiRect _rightBodyRect;
        private UiRect _closeBtnRect;
        private readonly UiRect[] _otherQuestCardRects = new UiRect[4];
        private UiRect _skillListHeaderRect;
        private UiRect _skillDetailHeaderRect;
        private UiRect _assignSkillButtonRect;
        private readonly UiRect[] _potentialButtonRects = new UiRect[4];
        private readonly UiRect[] _skillKeyButtonRects = new UiRect[10];
        private readonly UiRect[] _intrinsicSideButtonRects = new UiRect[2];
        private UiRect _intrinsicInputDialogRect;
        private UiRect _intrinsicInputCloseRect;
        private UiRect _intrinsicNormalButtonRect;
        private UiRect _intrinsicVipButtonRect;
        private readonly UiRect[] _equipmentSlotRects = new UiRect[16];
        private UiRect _inventoryDetailRect;
        private readonly UiRect[] _inventoryActionRects = new UiRect[4];
        private UiRect _clanChatHeaderRect;
        private UiRect _clanChatListRect;
        private UiRect _clanChatComposerRect;
        private UiRect _clanShareRect;
        private UiRect _clanSendRect;
        private UiRect _clanRightHeaderRect;
        private UiRect _clanRightBodyRect;
        private readonly UiRect[] _clanFunctionRects = new UiRect[6];
        private readonly UiRect[] _clanSideActionRects = new UiRect[4];
        private readonly UiRect[] _clanPotentialRects = new UiRect[6];
        private UiRect _clanUpgradeButtonRect;
        private UiRect _clanDialogRect;
        private UiRect _clanDialogCloseRect;
        private UiRect _clanDialogSubmitRect;
        private readonly UiRect[] _functionMenuRects = new UiRect[11];
        private readonly UiRect[] _functionZoneRects = new UiRect[20];
        private readonly UiRect[] _functionToggleRects = new UiRect[5];
        private readonly UiTabBar _discipleLeftTabBar = new UiTabBar();
        private readonly UiTabBar _discipleRightTabBar = new UiTabBar();
        private int _discipleLeftTab;
        private int _discipleRightTab;
        private int _discipleFocusArea;
        private int _selectedDiscipleRow;
        private int _selectedDiscipleEquipmentSlot = -1;
        private int _friendMode;
        private int _friendFocusArea;
        private int _selectedFriendRow;
        private int _selectedFriendMessage;
        private int _friendSearchToken;
        private int _friendInboxToken;
        private int _friendChatFieldFriendId = -1;
        private TField _friendSearchField;
        private TField _friendChatField;
        private TextFieldAdapter _friendSearchAdapter;
        private TextFieldAdapter _friendChatAdapter;
        private UiRect _friendHeaderRect;
        private UiRect _friendChatHeaderRect;
        private UiRect _friendMailRect;
        private UiRect _friendSearchRect;
        private UiRect _friendFindRect;
        private UiRect _friendHeadingRect;
        private UiRect _friendListRect;
        private UiRect _friendChatRect;
        private UiRect _friendComposerRect;
        private UiRect _friendLocationRect;
        private UiRect _friendSendRect;
        private UiRect _functionWorldChatListRect;
        private UiRect _functionWorldChatComposerRect;
        private UiRect _functionWorldChatSendRect;

        private const int MainTabCount = 7;
        private const int MainTabRowHeight = 48;
        private const int MaxFrameWidth = 460;
        private const int MaxFrameHeight = 242;
        private const int SidebarWidth = 70;
        private const int FooterHeight = 27;
        private const int MainTaskRowHeight = 33;
        private const int OtherTaskRowHeight = 34;
        private const int InventoryEquipmentSlotCount = 16;
        private const int InventoryEquipmentBorderInset = 2;
        private const int InventoryGridColumns = 5;
        private const int InventoryGridRowHeight = 36;
        private const int CompactListRowHeight = 31;
        private const int InventoryListRowHeight = CompactListRowHeight;
        private const int SkillRowHeight = 35;
        private const int PotentialStatRowCount = 5;
        private const int IntrinsicRowIndex = 5;
        private const int SkillTemplateStartRow = 6;
        private const int IntrinsicNpcId = 5;
        private const int MaxIntrinsicActionCount = 4;
        private const int IntrinsicListRowHeight = 35;
        private const int SkillKeyButtonCount = 10;
        private const int LastKnownMainTaskId = 29;
        private const int KeyboardFocusMainTabs = 0;
        private const int KeyboardFocusContent = 1;
        private const int InventoryFocusLeft = 0;
        private const int InventoryFocusBagTabs = 1;
        private const int InventoryFocusBagItems = 2;
        private const int InventoryFocusActions = 3;
        private const int PlayerProfileHeaderHeight = 96;
        private const int PlayerInfoLineHeight = 15;
        private const sbyte InventoryBagToBody = 4;
        private const sbyte InventoryBodyToBag = 5;
        private const sbyte InventoryBagToPet = 6;
        private const sbyte InventoryBagToPet2 = 8;
        private const sbyte InventoryWhereBody = 0;
        private const sbyte InventoryWhereBag = 1;
        private const int ClanViewMembers = 0;
        private const int ClanViewInfo = 1;
        private const int ClanViewTreasury = 2;
        private const int ClanViewPotential = 3;
        private const int ClanViewUpgrade = 4;
        private const int ClanViewHistory = 5;
        private const int ClanFocusFunctions = 0;
        private const int ClanFocusContent = 1;
        private const int ClanFocusSideActions = 2;
        private const int ClanFocusChat = 3;
        private const int ClanInputNone = 0;
        private const int ClanInputDepositGold = 1;
        private const int ClanInputDepositGem = 2;
        private const int ClanInputSlogan = 3;
        private const int ClanChatRowHeight = 43;
        private const int ClanMemberRowHeight = 29;
        private const int ClanPotentialRowHeight = 34;
        private const int ClanLedgerRowHeight = 38;
        private const int ClanStorageColumns = 5;
        private const int ClanStorageRowHeight = 36;
        private const int SkillFocusList = 0;
        private const int SkillFocusDetail = 1;
        private const int SkillFocusKeys = 2;
        private const int SkillFocusIntrinsicSide = 3;
        private const int FunctionFocusMenu = 0;
        private const int FunctionFocusContent = 1;
        private const int FunctionNotification = 0;
        private const int FunctionChangeZone = 1;
        private const int FunctionChangeFlag = 2;
        private const int FunctionActivity = 3;
        private const int FunctionCollection = 4;
        private const int FunctionWorldChat = 5;
        private const int FunctionMod = 6;
        private const int FunctionAccount = 7;
        private const int FunctionSettings = 8;
        private const int FunctionHistory = 9;
        private const int FunctionChangeAccount = 10;
        private const int FunctionViewDefault = 0;
        private const int FunctionViewNotifications = 1;
        private const int FunctionViewZones = 2;
        private const int FunctionViewFlags = 3;
        private const int FunctionViewActivityOverview = 4;
        private const int FunctionViewActivityDaily = 5;
        private const int FunctionViewActivityWeekly = 6;
        private const int FunctionViewActivitySources = 7;
        private const int FunctionViewWorldChat = 8;
        private const int FunctionViewToggles = 9;
        private const int FunctionViewCollection = 11;
        private const int FunctionViewAccount = 12;
        private const int FunctionViewSettings = 13;
        private const int FunctionViewHistory = 14;
        private const int FunctionViewChangeAccount = 15;

        private static Image[] _mainTabIcons;
        private static Image[] _otherTaskIcons;
        private static Image[] _statusIcons;

        private static readonly int[] PotentialIcons = new int[] { 567, 569, 568, 721, 719 };
        private static readonly int[] EquipmentVisualOrder = new int[] { 0, 1, 4, 2, 3, 5, 6, 7, 8, 9, 10, 11, 13, 12, 14, 15 };
        private static readonly string[] EquipmentSlotLabels = new string[]
        {
            "Áo", "Quần", "Găng", "Giày", "Rada", "Cải\ntrang",
            "Giáp\ntập\nluyện", "Pet", "Đeo\nlưng", "Ván\nbay", "Nhẫn",
            "Sách\ntuyệt\nkỹ", "Hào\nquang", "Danh\nhiệu", "Trống", "Trống"
        };
        private static readonly string[] ClanFunctionNames = new string[]
        {
            "Thành viên", "Thông tin", "Kho bang", "Tiềm năng", "Xin đậu", "Nâng cấp"
        };
        private static readonly string[] ClanPotentialNames = new string[]
        {
            "Sức đánh", "HP", "KI", "May mắn", "TNSM", "Vàng từ quái"
        };
        private static readonly string[] PotentialNames = new string[]
        {
            "HP gốc",
            "KI gốc",
            "Sức đánh gốc",
            "Giáp gốc",
            "Chí mạng gốc"
        };

        private static readonly string[] DefaultIntrinsicActionLabels = new string[]
        {
            "Mở nội tại",
            "Mở VIP",
            "Danh sách nội tại"
        };

        private static readonly int[] DefaultIntrinsicActionServerIndices = new int[] { 1, 2, 0 };

        private static readonly string[] MainTabNames = new string[]
        {
            "Nhiệm vụ",
            "Hành trang",
            "Kỹ năng",
            "Bang hội",
            "Chức năng",
            "Đệ tử",
            "Bạn bè"
        };

        private static readonly string[] FunctionNames = new string[]
        {
            "Thông báo", "Đổi khu",
            "Đổi cờ",
            "Năng động", "Sổ sưu tầm",
            "Chat thế giới", "Chức năng",
            "Tài khoản", "Cấu hình",
            "Lịch sử", "Đổi tài khoản"
        };

        // Matches the visual order in PaintOtherQuestGroup: daily, infinity, clan, fishing.
        private static readonly int[] OtherQuestNavigationOrder = new int[] { 0, 1, 3, 2 };

        // Canonical Main Task Names in Dragon Boy / Teamobi NRO (task_main_template)
        public static readonly string[] MainTaskNames = new string[]
        {
            "Nhiệm vụ đầu tiên",                 // 0
            "Nhiệm vụ tập luyện",                // 1
            "Nhiệm vụ tìm thức ăn",              // 2
            "Nhiệm vụ sao băng",                 // 3
            "Nhiệm vụ thử thách",                // 4
            "Nhiệm vụ thử thách",                // 5
            "Nhiệm vụ thử thách",                // 6
            "Nhiệm vụ giải cứu",                 // 7
            "Nhiệm vụ tìm ngọc",                 // 8
            "Nhiệm vụ bái sư",                   // 9
            "Nhiệm vụ thử sức",                  // 10
            "Nhiệm vụ gia tăng sức mạnh",        // 11
            "Nhiệm vụ xin phép",                 // 12
            "Nhiệm vụ gia nhập bang hội",        // 13
            "Nhiệm vụ bang hội đầu tiên",        // 14
            "Nhiệm vụ bang hội thứ 2",           // 15
            "Tiêu diệt quái vật",                // 16
            "Nhiệm vụ giúp đỡ Cui",              // 17
            "Nhiệm vụ bất khả thi",              // 18
            "Nhiệm vụ tìm diệt đệ tử",           // 19
            "Nhiệm vụ Tiểu đội sát thủ",         // 20
            "Nhiệm vụ chạm trán Fide đại ca",    // 21
            "Chú bé đến từ tương lai",           // 22
            "Chạm chán Robot sát thủ lần 1",     // 23
            "Chạm trán Robot sát thủ lần 2",     // 24
            "Chạm trán Robot sát thủ lần 3",     // 25
            "Chạm trán Xên bọ hung",             // 26
            "Cuộc dạo chơi của xên",             // 27
            "Cuộc đối đầu không cân sức",        // 28
            "Trở về quá khứ",                    // 29
            "Đạt 1 tỷ sức mạnh",                 // 30
            "Thử thách Thần Hủy Diệt",           // 31
            "Thu thập ngọc rồng siêu cấp"        // 32
        };

        // Guidance is kept for every task so completed rows remain as informative as the active row.
        public static readonly string[] CanonicalTaskGuides = new string[]
        {
            "Đi theo mũi tên chỉ dẫn, gặp người thân tại nhà, mở rương đồ, thu hoạch đậu thần rồi quay lại báo cáo.",
            "Mộc nhân ở ngay trước nhà tại làng. Đánh ngã 5 mộc nhân rồi về báo cáo; chạm nhanh hai lần vào mục tiêu để tấn công.",
            "Đến đồi hoang của hành tinh, tiêu diệt quái và nhặt đủ 10 đùi gà rồi quay về báo cáo.",
            "Học kỹ năng bay bằng tiềm năng, sau đó đi kiểm tra vật thể lạ vừa rơi xuống hành tinh.",
            "Khủng long mẹ ở Trái Đất, lợn lòi mẹ ở Namếc và quỷ đất mẹ ở Xayda. Dùng tàu vũ trụ để đổi hành tinh.",
            "Lợn lòi mẹ ở Namếc, khủng long mẹ ở Trái Đất và quỷ đất mẹ ở Xayda. Dùng tàu vũ trụ để đổi hành tinh.",
            "Quỷ đất mẹ ở Xayda, khủng long mẹ ở Trái Đất và lợn lòi mẹ ở Namếc. Dùng tàu vũ trụ để đổi hành tinh.",
            "Đến khu vực được chỉ dẫn, hạ 20 quái bay rồi quay về làng báo cáo.",
            "Ngọc rồng 7 sao đã bị quái vật cướp. Đánh bại chúng, tìm lại ngọc và mang về giao cho người thân.",
            "Tìm đường đến Đảo Kamê, Đảo Guru hoặc Vách núi đen; nói chuyện với sư phụ của hành tinh để xin làm đệ tử.",
            "Luyện đủ sức mạnh, tiêu diệt số quái được yêu cầu để thể hiện thực lực rồi báo cáo với sư phụ.",
            "Ra ngoài luyện tập và đạt lần lượt các mốc sức mạnh được giao, sau đó trở về nhà.",
            "Quay về nhà gặp ông Gôhan, Moori hoặc Paragus để xin phép tham gia bang hội.",
            "Tạo hoặc gia nhập một bang hội có đồng đội thiện chí, cùng nhau làm nhiệm vụ rồi báo cáo với sư phụ.",
            "Phối hợp với một đồng đội để tiến trình nhanh gấp đôi. Heo rừng ở Rừng Bamboo, heo da xanh ở Núi hoa vàng, heo Xayda ở Rừng cọ; dùng tàu vũ trụ để di chuyển.",
            "Phối hợp với một đồng đội để tiến trình nhanh gấp đôi; tiêu diệt Bulon, Ukulele và Quỷ mập rồi quay về báo cáo.",
            "Tới các hành tinh, lần lượt tiêu diệt Tambourine, Drum và Akkuman để giải cứu thường dân rồi báo cáo với sư phụ.",
            "Tìm đường tới Thành phố Vegeta trên hành tinh Xayda, gặp và nói chuyện với Cui.",
            "Đạt 50 triệu sức mạnh rồi tới Xayda tiêu diệt Nappa, Soldier, Appule, Raspberry và Thằn lằn xanh.",
            "Tiêu diệt Kuku, Mập Đầu Đinh và Rambo tại Xayda. Nếu chưa tìm thấy, hãy gặp Cui ở Thành phố Vegeta để hỏi vị trí.",
            "Tới Xayda tiêu diệt các thành viên Tiểu Đội Sát Thủ do Fide gọi đến, sau đó báo cáo với sư phụ.",
            "Luyện tập đạt 2 tỷ sức mạnh rồi lên đường tìm và đánh bại các hình thái của Fide đại ca.",
            "Đến Trái Đất, tìm người lạ tại Rừng Bamboo, Rừng dương xỉ và Nam Kamê; đưa thuốc cho Quy Lão rồi theo Ca Lích tới tương lai diệt bọ hung con.",
            "Tìm Rôbốt Sát Thủ tại Thành phố phía Nam, Đảo Balê hoặc Cao nguyên; cùng hai đồng bang diệt Xên con cấp 3 rồi báo Bunma tương lai.",
            "Trở về quá khứ, tới sân sau siêu thị, tiêu diệt Android 15, Android 14 và Android 13 rồi báo Bunma tương lai.",
            "Đến Thành phố phía Bắc, Ngọn núi phía Bắc và Thung lũng phía Bắc; diệt Rôbốt Sát Thủ và Xên con cấp 5 rồi báo Bunma tương lai.",
            "Đến thị trấn Ginder, tiêu diệt các hình thái Xên Bọ Hung và Xên con cấp 8 rồi báo Bunma tương lai.",
            "Nâng sức đánh gốc, thu thập Capsule kì bí, tới võ đài Xên Bọ Hung và cẩn thận với những vị khách nguy hiểm.",
            "Vào 12 giờ trưa hằng ngày, đến map Đại hội võ thuật gặp Ô Sin và làm theo chỉ dẫn.",
            "Gặp Ca Lích tại tương lai, dùng cỗ máy thời gian trở về quá khứ rồi báo cáo với Quy Lão Kame.",
            "Luyện tập không ngừng để đạt 1 tỷ sức mạnh rồi quay về báo cáo với sư phụ.",
            "Đến Thánh địa Beerus, vượt thử thách của Whis và giao đấu với Thần Hủy Diệt Beerus.",
            "Tìm đủ 7 viên ngọc rồng siêu cấp trong vũ trụ để triệu hồi Rồng Thần Zarama."
        };

        // Canonical Sub-Steps for all Main Tasks
        public static readonly string[][] CanonicalTaskSubSteps = new string[][]
        {
            new string[] { "Di chuyển tới mũi tên chỉ dẫn", "Hãy đi đến nhà ông Gôhan / Moori / Paragus", "Nói chuyện với ông Gôhan / Moori / Paragus", "Mở rương đồ", "Thu hoạch đậu thần", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh ngã 5 mộc nhân", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Thu thập 10 đùi gà tại đồi hoang", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Sử dụng tiềm năng học kỹ năng bay", "Đi khám phá vật thể lạ rơi xuống", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh 3 con khủng long mẹ (Trái Đất)", "Đánh 3 con lợn lòi mẹ (Namếc)", "Đánh 3 con quỷ đất mẹ (Xayda)", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh 3 con lợn lòi mẹ (Namếc)", "Đánh 3 con khủng long mẹ (Trái Đất)", "Đánh 3 con quỷ đất mẹ (Xayda)", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đánh 3 con quỷ đất mẹ (Xayda)", "Đánh 3 con khủng long mẹ (Trái Đất)", "Đánh 3 con lợn lòi mẹ (Namếc)", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đạt 16.000 sức mạnh", "Đánh bại 20 con quái bay (Phi long / Quỷ bay / Thằn lằn bay)", "Nói chuyện với NPC tại làng", "Báo cáo với ông Gôhan / Moori / Paragus" },
            new string[] { "Đạt 40.000 sức mạnh", "Tìm kiếm viên ngọc rồng 7 sao", "Đem ngọc rồng về cho ông Gôhan / Moori / Paragus" },
            new string[] { "Lên đường tới Đảo Kamê / Đảo Guru / Vách núi đen", "Chào hỏi và bái Quy Lão Kame / Guru / Vua Vegeta làm sư phụ" },
            new string[] { "Đạt 200.000 sức mạnh", "Tiêu diệt 10 con quái thể hiện sức mạnh", "Báo cáo kết quả với sư phụ" },
            new string[] { "Đạt 500.000 sức mạnh", "Đạt 550.000 sức mạnh", "Đạt 600.000 sức mạnh", "Đi về nhà thăm ông Gôhan / Moori / Paragus" },
            new string[] { "Đi về nhà gặp ông Gôhan / Moori / Paragus", "Nói chuyện xin phép gia nhập bang hội", "Báo cáo lại cho sư phụ" },
            new string[] { "Tạo hoặc gia nhập 1 bang hội có từ 2 thành viên", "Báo cáo hoàn tất cho sư phụ" },
            new string[] { "Tiêu diệt 30 con heo rừng tại Rừng Bamboo", "Tiêu diệt 30 con heo da xanh tại Núi hoa vàng", "Tiêu diệt 30 con heo xayda tại Rừng cọ", "Quay về báo cáo nhiệm vụ cho sư phụ" },
            new string[] { "Tiêu diệt 30 Bulon", "Tiêu diệt 30 Ukulele", "Tiêu diệt 30 Quỷ mập", "Quay về báo cáo nhiệm vụ cho sư phụ" },
            new string[] { "Tiêu diệt Tambourine", "Tiêu diệt Drum", "Tiêu diệt Akkuman", "Quay về báo cáo nhiệm vụ cho sư phụ" },
            new string[] { "Tìm đường tới Thành Phố Vegeta", "Gặp gỡ và nói chuyện với Cui" },
            new string[] { "Tiêu diệt 500 Nappa", "Tiêu diệt 400 Soldier", "Tiêu diệt 300 Appule", "Tiêu diệt 200 Raspberry", "Tiêu diệt 100 Thằn lằn xanh", "Báo cáo kết quả với sư phụ" },
            new string[] { "Tiêu diệt Kuku", "Tiêu diệt Mập Đầu Đinh", "Tiêu diệt Rambo", "Trả nhiệm vụ cho sư phụ" },
            new string[] { "Đạt 600 triệu sức mạnh", "Tiêu diệt Số 4 (Guldo)", "Tiêu diệt Số 3 (Recoome)", "Tiêu diệt Số 2 (Burter)", "Tiêu diệt Số 1 (Jeice)", "Tiêu diệt Tiểu Đội Trưởng (Ginyu)", "Báo cáo với sư phụ" },
            new string[] { "Đạt 2 tỷ sức mạnh", "Tiêu diệt Fide cấp 1", "Tiêu diệt Fide cấp 2", "Tiêu diệt Fide cấp 3", "Báo cáo với sư phụ" },
            new string[] { "Báo cáo với ông Gôhan / Moori / Paragus", "Đi tìm vị khách lạ (Ca Lích)", "Đưa thuốc trợ tim cho Quy Lão", "Đến tương lai gặp Bunma", "Diệt 1000 xên con cấp 1", "Báo với Bunma tương lai" },
            new string[] { "Đến điểm hẹn tìm Rôbốt Sát Thủ", "Tiêu diệt Số 2 (Android 19)", "Tiêu diệt Số 1 (Android 20)", "Diệt 900 xên con cấp 3", "Báo với Bunma tương lai" },
            new string[] { "Đến sân sau siêu thị", "Tiêu diệt Android 15", "Tiêu diệt Android 14", "Tiêu diệt Android 13", "Báo với Bunma tương lai" },
            new string[] { "Đi tìm Píc Póc", "Tiêu diệt Póc", "Tiêu diệt Píc", "Tiêu diệt Kinh Kong", "Diệt 800 xên con cấp 5", "Báo với Bunma tương lai" },
            new string[] { "Đến thị trấn Ginder", "Tiêu diệt Xên Bọ Hung cấp 1", "Tiêu diệt Xên Bọ Hung cấp 2", "Tiêu diệt Xên Bọ Hung hoàn thiện", "Diệt 700 xên con cấp 8", "Báo với Bunma tương lai" },
            new string[] { "Nâng sức đánh gốc lên 10.000", "Thu thập 50 Capsule kì bí", "Đến võ đài Xên Bọ Hung", "Tiêu diệt 7 đứa con của Xên", "Tiêu diệt Siêu Bọ Hung", "Báo với Bunma tương lai" },
            new string[] { "Gặp và đi theo Ôsin tại Đại Hội Võ Thuật", "Hạ vua địa ngục Drabura (0/10)", "Hạ Pui Pui (0/10)", "Hạ Pui Pui lần 2 (0/10)", "Hạ Yacôn (0/10)", "Hạ Drabura lần 2 (0/10)", "Hạ Ma Bư (0/10)", "Báo cáo kết quả với Ôsin" },
            new string[] { "Gặp Ca Lích tại tương lai", "Khởi hành cỗ máy thời gian trở về quá khứ", "Báo cáo với Quy Lão Kame" },
            new string[] { "Luyện tập không ngừng đạt mốc 1 tỷ sức mạnh", "Báo cáo với sư phụ" },
            new string[] { "Đến Thánh địa Beerus", "Vượt qua thử thách của Whis", "Giao đấu cùng Thần Hủy Diệt Beerus" },
            new string[] { "Tìm kiếm 7 viên ngọc rồng siêu cấp vũ trụ", "Triệu hồi Rồng Thần Siêu Cấp Zarama", "Ước 1 điều ước vĩ đại" }
        };

        // Canonical Rewards for all Main Tasks
        public static readonly string[][] CanonicalTaskRewards = new string[][]
        {
            new string[] { "500 sức mạnh", "500 tiềm năng" },
            new string[] { "1.000 sức mạnh", "1.000 tiềm năng", "200 triệu vàng" },
            new string[] { "1.500 sức mạnh", "1.500 tiềm năng", "300 triệu vàng", "Học kỹ năng bay" },
            new string[] { "2.000 sức mạnh", "2.000 tiềm năng", "400 triệu vàng" },
            new string[] { "4.000 sức mạnh", "4.000 tiềm năng" },
            new string[] { "4.000 sức mạnh", "4.000 tiềm năng" },
            new string[] { "4.000 sức mạnh", "4.000 tiềm năng" },
            new string[] { "8.000 sức mạnh", "8.000 tiềm năng" },
            new string[] { "15.000 sức mạnh", "15.000 tiềm năng" },
            new string[] { "20.000 tiềm năng", "Bái sư thành công" },
            new string[] { "30.000 tiềm năng", "500.000 vàng" },
            new string[] { "50.000 tiềm năng", "1.000.000 vàng" },
            new string[] { "80.000 tiềm năng", "2.000.000 vàng" },
            new string[] { "100.000 tiềm năng", "Capsule 1 chỗ" },
            new string[] { "150.000 tiềm năng", "Đùi gà nướng" },
            new string[] { "200.000 tiềm năng", "Rương đồ thần bí" },
            new string[] { "300.000 tiềm năng", "10 ngọc xanh" },
            new string[] { "500.000 tiềm năng", "Capsule đặc biệt" },
            new string[] { "1.000.000 tiềm năng", "50 ngọc xanh" },
            new string[] { "2.000.000 tiềm năng", "Đậu thần cấp 7" },
            new string[] { "5.000.000 tiềm năng", "Găng tay vàng" },
            new string[] { "10.000.000 tiềm năng", "100 ngọc xanh" },
            new string[] { "20.000.000 tiềm năng", "Bộ ngọc rồng 3 sao" },
            new string[] { "30.000.000 tiềm năng", "Capsule Vàng" },
            new string[] { "40.000.000 tiềm năng", "Radar dò ngọc" },
            new string[] { "50.000.000 tiềm năng", "200 ngọc xanh" },
            new string[] { "60.000.000 tiềm năng", "Cải trang Xên bọ hung" },
            new string[] { "80.000.000 tiềm năng", "Capsule Kỳ Bí" },
            new string[] { "100.000.000 tiềm năng", "Kiếm Z huyền thoại" },
            new string[] { "150.000.000 tiềm năng", "Bộ ngọc rồng Namếc" },
            new string[] { "200.000.000 tiềm năng", "Danh hiệu Chiến Thần" },
            new string[] { "300.000.000 tiềm năng", "Đồ Thần Linh" },
            new string[] { "500.000.00 điều ước Rồng Siêu Cấp", "1.000 ngọc xanh" }
        };

        // Multi-NPC Quest Model
        public class NpcQuest
        {
            public string key;
            public string categoryName;
            public string npcName;
            public bool hasQuest;
            public string title;
            public string goal;
            public int count;
            public int maxCount;
            public string defaultHint;
            public string summary;
            public string[] rewards;
            public bool isComplete;
            public int iconType; // 0: BoMong, 1: Kanao, 2: Fishing, 3: Clan
        }

        public static NpcQuest questBoMong = new NpcQuest
        {
            key = "bomong",
            categoryName = "Nhiệm vụ hằng ngày",
            npcName = "Bò Mộng",
            hasQuest = false,
            title = "Nhiệm vụ hằng ngày",
            goal = "Chưa nhận nhiệm vụ hằng ngày từ Bò Mộng.",
            iconType = 0,
            rewards = new string[] { "10.000 Ngọc xanh", "50.000.000 Vàng", "Điểm năng động hằng ngày", "Huy hiệu Nông Dân Chăm Chỉ (Cấp Địa Ngục)" },
            summary = "Hoàn thành nhiệm vụ mỗi ngày để nhận thêm phần thưởng.",
            defaultHint = "Đến gặp NPC Bò Mộng tại Làng Aru / Làng Mori / Làng Kakarot để chọn độ khó (Dễ, Bình thường, Khó) và nhận nhiệm vụ. Sau khi hoàn thành mục tiêu, quay lại gặp Bò Mộng để trả nhiệm vụ và nhận thưởng."
        };

        public static NpcQuest questKanao = new NpcQuest
        {
            key = "kanao",
            categoryName = "Nhiệm vụ hằng ngày",
            npcName = "Kanao",
            hasQuest = false,
            title = "Nhiệm vụ Vô Hạn Thành",
            goal = "Chưa nhận nhiệm vụ Vô Hạn Thành từ Kanao.",
            iconType = 1,
            rewards = new string[] { "Huyết Quỷ", "Rương Vô Hạn Thành", "50.000.000 Tiềm năng", "Điểm cống hiến sự kiện" },
            summary = "Tiêu diệt mục tiêu trong Vô Hạn Thành rồi quay lại Kanao.",
            defaultHint = "Đến gặp Kanao tại Làng Aru hoặc map 187 (Vô Hạn Thành) để nhận nhiệm vụ diệt quái ngẫu nhiên. Di chuyển vào map tương ứng trong Vô Hạn Thành để săn quái vật. Khi hoàn thành, gặp lại Kanao và chọn Nhận thưởng."
        };

        public static NpcQuest questFishing = new NpcQuest
        {
            key = "fishing",
            categoryName = "Nhiệm vụ câu cá",
            npcName = "Ngư Dân",
            hasQuest = false,
            title = "Nhiệm vụ câu cá",
            goal = "Chưa nhận nhiệm vụ câu cá.",
            iconType = 2,
            rewards = new string[] { "60 Xu Ngư Phủ", "5 Mồi Tôm Biển", "1 Huy Hiệu Ngư Phủ", "Điểm kinh nghiệm câu cá" },
            summary = "Câu đủ số cá yêu cầu rồi trở về gặp Ngư Dân.",
            defaultHint = "Đến Làng Chài hoặc Đảo Kamê gặp Ngư Dân để nhận nhiệm vụ câu cá theo độ khó (Dễ, Vừa, Khó, Cực khó). Chuẩn bị cần câu và mồi câu thích hợp tại các ngư trường. Khi câu cá hoàn thành, quay lại gặp Ngư Dân để nhận thưởng."
        };

        public static NpcQuest questClan = new NpcQuest
        {
            key = "clan",
            categoryName = "Nhiệm vụ bang hội",
            npcName = "Sư Phụ",
            hasQuest = false,
            title = "Nhiệm vụ bang hội",
            goal = "Chưa nhận nhiệm vụ bang hội.",
            iconType = 3,
            rewards = new string[] { "Capsule Bang hội", "Tiềm năng sức mạnh", "Điểm cống hiến bang hội", "Quỹ phúc lợi bang" },
            summary = "Cùng thành viên hoàn thành mục tiêu của bang hội.",
            defaultHint = "Yêu cầu đã gia nhập một Bang hội có từ 2 thành viên trở lên. Đến gặp Sư phụ của hành tinh (Quy Lão Kame tại Đảo Kamê, Trưởng lão Guru tại Đảo Guru, hoặc Vua Vegeta tại Hành tinh Vegeta) để nhận nhiệm vụ bang. Khi hoàn thành, quay về báo cáo với Bang hội."
        };

        public static readonly NpcQuest[] allOtherQuests = new NpcQuest[]
        {
            questBoMong,
            questKanao,
            questFishing,
            questClan
        };

        public static void Open()
        {
            if (_instance == null)
            {
                _instance = new CustomMenuScr();
            }
            if (_isOpen) return;
            _instance._previousScreen = GameCanvas.currentScreen;
            _instance.Init();
            _isOpen = true;
            GameCanvas.clearKeyPressed();
            GameCanvas.clearKeyHold();
        }

        public static void Toggle()
        {
            if (_isOpen && _instance != null)
            {
                _instance.Close();
                return;
            }
            Open();
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            CloseClanDialog();
            _clanChatFocused = false;
            _clanChatField?.setFocus(false);
            _functionWorldChatFocused = false;
            _functionWorldChatField?.setFocus(false);
            if (_selectedMainTab == 6) LeaveFriendTab();
            else BlurFriendInputs();
            _inputContext?.ConsumePointer();
            _inputContext?.Reset();
            _previousScreen = null;
            GameCanvas.clearKeyPressed();
            GameCanvas.clearKeyHold();
        }

        public static void UpdateOverlay()
        {
            if (!_isOpen || _instance == null) return;
            if (_instance._previousScreen != null && !object.ReferenceEquals(GameCanvas.currentScreen, _instance._previousScreen))
            {
                _instance.Close();
                return;
            }
            _instance.update();
            _instance.updateKey();
        }

        public static void PaintOverlay(mGraphics g)
        {
            if (!_isOpen || _instance == null) return;
            if (_instance._previousScreen != null && !object.ReferenceEquals(GameCanvas.currentScreen, _instance._previousScreen)) return;
            _instance.paint(g);
        }

        public static bool HandleMouseWheel(int wheelDelta)
        {
            if (wheelDelta == 0 || !_isOpen || _instance == null) return false;

            int pointerX = GameCanvas.pxMouse;
            int pointerY = GameCanvas.pyMouse;
            if (_instance._clanDialogMode != ClanInputNone || _instance._showIntrinsicInput)
                return _instance._frameRect.Contains(pointerX, pointerY);

            if (_instance._leftScrollAdapter != null
                && _instance._leftScrollAdapter.Viewport.Contains(pointerX, pointerY))
            {
                _instance._leftScrollAdapter.ScrollByWheel(wheelDelta);
                return true;
            }
            if (_instance._mainTabScrollAdapter.Viewport.Contains(pointerX, pointerY))
            {
                _instance._mainTabScrollAdapter.ScrollByWheel(wheelDelta);
                return true;
            }
            if (_instance._rightScrollAdapter != null
                && _instance._rightScrollAdapter.Viewport.Contains(pointerX, pointerY))
            {
                if (_instance._selectedMainTab == 6 && wheelDelta > 0)
                    _instance._friendChatAtBottom = false;
                _instance._rightScrollAdapter.ScrollByWheel(wheelDelta);
                return true;
            }
            return _instance._frameRect.Contains(pointerX, pointerY);
        }

        private void Init()
        {
            _inputContext = new UiInputContext();
            if (!_hasInitializedState)
            {
                _selectedMainTab = 0;
                _selectedSubTab = 0;
                _selectedInventoryLeftTab = 0;
                _selectedInventoryBagTab = 0;
                _inventoryFocusArea = InventoryFocusLeft;
                _selectedInventoryBodySlot = -1;
                _selectedInventoryBagSlot = -1;
                _selectedInventoryAction = 0;
                _showInventoryDetail = false;
                _selectedClanView = ClanViewMembers;
                _clanFocusArea = ClanFocusFunctions;
                _selectedClanFunction = 0;
                _selectedClanRow = 0;
                _selectedClanSideAction = 0;
                _selectedClanStorageSlot = -1;
                _clanStorageLoaded = false;
                _clanChatFocused = false;
                _clanDialogMode = ClanInputNone;
                _clanDialogFocus = 0;
                _clanDialogField = null;
                _selectedSkillRow = -1;
                _showSkillKeyPicker = false;
                _skillFocusArea = SkillFocusList;
                _selectedPotentialAction = -1;
                _selectedIntrinsicAction = -1;
                _intrinsicActionCount = 0;
                _intrinsicDialogText = string.Empty;
                for (int i = 0; i < _intrinsicActionLabels.Length; i++)
                {
                    _intrinsicActionLabels[i] = null;
                    _intrinsicActionServerIndices[i] = -1;
                }
                _waitingIntrinsicMenu = false;
                _waitingIntrinsicList = false;
                _showIntrinsicList = false;
                _showIntrinsicConfirmation = false;
                _expectingIntrinsicConfirmation = false;
                _selectedIntrinsicListIndex = -1;
                _selectedIntrinsicSideAction = 0;
                _showIntrinsicInput = false;
                _intrinsicInputFocus = 0;
                _intrinsicInputField = null;
                _selectedSkillKeyIndex = 0;
                _selectedFunction = -1;
                _functionView = FunctionViewDefault;
                _functionFocusArea = FunctionFocusMenu;
                _selectedFunctionRow = 0;
                _functionWorldChatFocused = false;
                _functionWorldChatField = null;
                _friendMode = FriendModeFriends;
                _friendFocusArea = FriendFocusList;
                _selectedFriendRow = 0;
                _selectedFriendMessage = 0;
                _friendSearchToken = 0;
                _friendInboxToken = 0;
                _keyboardFocus = KeyboardFocusContent;
                _hasInitializedState = true;
            }
            EnsureClanChatField();
            EnsureAssets();

            int screenW = GameCanvas.w;
            int screenH = GameCanvas.h;

            int frameW = System.Math.Min(MaxFrameWidth, screenW - 16);
            int frameH = System.Math.Min(MaxFrameHeight, screenH - 16);
            if (frameW < 360) frameW = screenW - 8;
            if (frameH < 220) frameH = screenH - 8;

            int frameX = (screenW - frameW) / 2;
            int frameY = (screenH - frameH) / 2;
            _frameRect = new UiRect(frameX, frameY, frameW, frameH);

            int contentX = frameX + SidebarWidth;
            int contentW = frameW - SidebarWidth;
            int contentH = frameH - FooterHeight;

            _footerRect = new UiRect(contentX, frameY + contentH, contentW, FooterHeight);
            _closeBtnRect = new UiRect(_footerRect.X + _footerRect.Width - 24, _footerRect.Y + 2, 22, 22);

            _tabBarRect = new UiRect(frameX, frameY, SidebarWidth, frameH);
            _mainTabScrollAdapter.Reset();
            _mainTabScrollAdapter.Configure(_tabBarRect, MainTabCount, MainTabRowHeight);
            _mainTabScrollAdapter.ScrollToIndex(_selectedMainTab);
            _contentRect = new UiRect(contentX, frameY, contentW, contentH);

            int columnWidth = (contentW - 12) / 2;
            int leftX = contentX + 4;
            int subTabW = (columnWidth - 4) / 2;
            _subTab0Rect = new UiRect(leftX, frameY + 4, subTabW, 22);
            _subTab1Rect = new UiRect(_subTab0Rect.X + subTabW + 4, frameY + 4, subTabW, 22);
            _inventoryLeftTab0Rect = _subTab0Rect;
            _inventoryLeftTab1Rect = _subTab1Rect;
            _skillListHeaderRect = new UiRect(leftX, frameY + 4, columnWidth, 22);
            _leftColRect = new UiRect(leftX, frameY + 31, columnWidth, contentH - 34);

            int rightX = leftX + columnWidth + 4;
            _rightColRect = new UiRect(rightX, frameY + 4, columnWidth, contentH - 8);
            _skillDetailHeaderRect = new UiRect(rightX, frameY + 4, columnWidth, 22);
            int inventoryBagTabW = (columnWidth - 4) / 2;
            _inventoryBagTab0Rect = new UiRect(rightX, frameY + 4, inventoryBagTabW, 22);
            _inventoryBagTab1Rect = new UiRect(_inventoryBagTab0Rect.X + inventoryBagTabW + 4, frameY + 4, inventoryBagTabW, 22);
            ConfigureInventoryTabBars();
            ConfigureDiscipleTabBars();
            _rightBodyRect = new UiRect(_rightColRect.X + 2, _rightColRect.Y + 26, _rightColRect.Width - 4, _rightColRect.Height - 28);
            ConfigureEquipmentSlotRects();
            ConfigureInventoryDetailRects();
            ConfigureSkillActionRects();
            ConfigureClanRects();
            ConfigureFunctionRects();
            ConfigureFriendRects();

            // Auto-select active main task
            Task currentTask = (Char.myCharz() != null) ? Char.myCharz().taskMaint : null;
            if (currentTask != null)
            {
                _selectedTaskPosition = GetMainTaskPosition((int)currentTask.taskId);
            }

            SelectFirstAvailableOtherQuest();
            ConfigureScrollAdapters();
            if (_selectedMainTab == 0 && _selectedSubTab == 0)
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
            else if (_selectedMainTab == 3)
                RefreshClanData();
            else if (_selectedMainTab == 4)
                EnterFunctionTab();
            else if (_selectedMainTab == 5)
                EnterDiscipleTab();
            else if (_selectedMainTab == 6)
                EnterFriendTab();
        }

        private static void DismissIntrinsicNpcOverlay()
        {
            if (Char.chatPopup != null)
            {
                Effect2.vEffect2.removeElement(Char.chatPopup);
                Char.chatPopup = null;
            }
            GameCanvas.menu?.doCloseMenu();
        }

        private static void EnsureAssets()
        {
            if (_mainTabIcons == null)
            {
                _mainTabIcons = new Image[]
                {
                    GameCanvas.loadImage("/custom_menu/main_task.png"),
                    GameCanvas.loadImage("/custom_menu/main_inventory.png"),
                    GameCanvas.loadImage("/custom_menu/main_skill.png"),
                    GameCanvas.loadImage("/custom_menu/main_clan.png"),
                    GameCanvas.loadImage("/custom_menu/main_function.png"),
                    GameCanvas.loadImage("/custom_menu/main_disciple.png"),
                    GameCanvas.loadImage("/custom_menu/main_friend.png")
                };
            }
            if (_otherTaskIcons == null)
            {
                _otherTaskIcons = new Image[]
                {
                    GameCanvas.loadImage("/custom_menu/task_daily.png"),
                    GameCanvas.loadImage("/custom_menu/task_infinity.png"),
                    GameCanvas.loadImage("/custom_menu/task_fishing.png"),
                    GameCanvas.loadImage("/custom_menu/task_clan.png"),
                    GameCanvas.loadImage("/custom_menu/task_other.png")
                };
            }
            if (_statusIcons == null)
            {
                _statusIcons = new Image[]
                {
                    GameCanvas.loadImage("/custom_menu/status_tick.png"),
                    GameCanvas.loadImage("/custom_menu/status_lock.png")
                };
            }
        }

        private void ConfigureScrollAdapters()
        {
            if (_leftScrollAdapter == null) _leftScrollAdapter = new UiScrollList();
            if (_rightScrollAdapter == null) _rightScrollAdapter = new UiScrollList();

            if (_selectedMainTab == 1)
            {
                if (_selectedInventoryLeftTab == 1)
                {
                    int infoHeight = GetInventoryInfoContentHeight();
                    _leftScrollAdapter.Configure(_leftColRect, (infoHeight + 9) / 10, 10);
                }
                else
                {
                    _leftScrollAdapter.Configure(UiRect.Empty, 0, 1);
                }

                int bagRows = GetInventoryBagRowCount();
                int rowHeight = ModFunc.isInventory ? InventoryGridRowHeight : InventoryListRowHeight;
                _rightScrollAdapter.Configure(_rightBodyRect, bagRows, rowHeight);
                return;
            }

            if (_selectedMainTab == 2)
            {
                _leftScrollAdapter.Configure(_leftColRect, GetSkillRowCount(), SkillRowHeight);
                _rightScrollAdapter.Configure(_rightBodyRect, _showIntrinsicList ? GetIntrinsicListCount() : 0,
                    _showIntrinsicList ? IntrinsicListRowHeight : 10);
                return;
            }

            if (_selectedMainTab == 3)
            {
                _leftScrollAdapter.Configure(_clanChatListRect, ClanMessage.vMessage.size(), ClanChatRowHeight);
                int itemCount = GetClanContentItemCount();
                int rowHeight = GetClanContentRowHeight();
                _rightScrollAdapter.Configure(GetClanScrollableBodyRect(), itemCount, rowHeight);
                return;
            }

            if (_selectedMainTab == 4)
            {
                UiRect viewport = GetFunctionScrollableViewport();
                int rowHeight = GetFunctionScrollableRowHeight();
                _leftScrollAdapter.Configure(UiRect.Empty, 0, 1);
                if (_functionView == FunctionViewNotifications)
                {
                    int totalH = GetFunctionNotificationTotalHeight();
                    _rightScrollAdapter.Configure(viewport, totalH, 1);
                }
                else
                {
                    _rightScrollAdapter.Configure(viewport, GetFunctionScrollableItemCount(), rowHeight);
                }
                return;
            }

            if (_selectedMainTab == 5)
            {
                _leftScrollAdapter.Configure(_leftColRect,
                    _discipleLeftTab == 0 ? GetDiscipleEquipmentCount() : 0, CompactListRowHeight);
                _rightScrollAdapter.Configure(_rightBodyRect,
                    _discipleRightTab == 0 ? GetDiscipleSkillRowCount() : GetDiscipleStatusCount(),
                    _discipleRightTab == 0 ? 30 : 29);
                return;
            }

            if (_selectedMainTab == 6)
            {
                _leftScrollAdapter.Configure(_friendListRect, GetFriendRowCount(), FriendRowHeight);
                _rightScrollAdapter.Configure(_friendChatRect, GetFriendMessageCount(), FriendMessageRowHeight);
                return;
            }

            if (_selectedMainTab != 0)
            {
                _leftScrollAdapter.Configure(UiRect.Empty, 0, 1);
                _rightScrollAdapter.Configure(UiRect.Empty, 0, 1);
                return;
            }

            if (_selectedSubTab == 0)
            {
                int rowCount = GetMainTaskRowCount();
                _leftScrollAdapter.Configure(GetLeftListViewport(), rowCount, MainTaskRowHeight);
                int rightH = GetRightContentHeight();
                _rightScrollAdapter.Configure(_rightBodyRect, (rightH / 10) + 1, 10);
            }
            else
            {
                int totalH = GetOtherListHeight();
                _leftScrollAdapter.Configure(_leftColRect, (totalH / 10) + 1, 10);
                int rightH = GetRightContentHeight();
                _rightScrollAdapter.Configure(_rightBodyRect, (rightH / 10) + 1, 10);
            }
        }

        public override void update()
        {
            base.update();
            if (_waitingIntrinsicList && HasIntrinsicListData()) EnterIntrinsicListView();
            if (_showIntrinsicList && GameCanvas.panel != null && GameCanvas.panel.isShow) GameCanvas.panel.hide();
            if (_showIntrinsicList && _rightScrollAdapter != null
                && _rightScrollAdapter.ItemCount != GetIntrinsicListCount()) ConfigureScrollAdapters();
            if (_showIntrinsicInput) _intrinsicInputField?.update();
            if (_selectedMainTab == 3)
            {
                _clanChatField?.update();
                if (_clanDialogMode != ClanInputNone) _clanDialogField?.update();
                int chatCount = ClanMessage.vMessage.size();
                int contentCount = GetClanContentItemCount();
                if (_leftScrollAdapter != null && _leftScrollAdapter.ItemCount != chatCount
                    || _rightScrollAdapter != null && _rightScrollAdapter.ItemCount != contentCount)
                    ConfigureScrollAdapters();
            }
            if (_selectedMainTab == 4)
            {
                if (_functionWorldChatFocused) _functionWorldChatField?.update();
                int expectedFunctionScrollCount = _functionView == FunctionViewNotifications
                    ? GetFunctionNotificationTotalHeight()
                    : GetFunctionScrollableItemCount();
                if (_rightScrollAdapter != null && _rightScrollAdapter.ItemCount != expectedFunctionScrollCount)
                    ConfigureScrollAdapters();
            }
            if (_selectedMainTab == 5) RefreshDiscipleInfo();
            if (_selectedMainTab == 5 && _leftScrollAdapter != null && _rightScrollAdapter != null)
            {
                int equipmentCount = _discipleLeftTab == 0 ? GetDiscipleEquipmentCount() : 0;
                int rightCount = _discipleRightTab == 0 ? GetDiscipleSkillRowCount() : GetDiscipleStatusCount();
                if (_leftScrollAdapter.ItemCount != equipmentCount || _rightScrollAdapter.ItemCount != rightCount)
                    ConfigureScrollAdapters();
            }
            if (_selectedMainTab == 6)
            {
                _friendSearchAdapter?.Update();
                _friendChatAdapter?.Update();
                int friendRowCount = GetFriendRowCount();
                int messageCount = GetFriendMessageCount();
                bool newMessage = _rightScrollAdapter != null
                    && _rightScrollAdapter.ItemCount != messageCount;
                if (_leftScrollAdapter != null && _rightScrollAdapter != null
                    && (_leftScrollAdapter.ItemCount != friendRowCount || newMessage))
                {
                    ConfigureScrollAdapters();
                    if (newMessage && _friendChatAtBottom && messageCount > 0)
                        _rightScrollAdapter.ScrollToIndex(messageCount - 1);
                }
                MaybeLoadFriendNextPage();
            }
            if (_selectedMainTab == 1 && _selectedInventoryLeftTab == 1 && _leftScrollAdapter != null)
            {
                int infoRows = (GetInventoryInfoContentHeight() + 9) / 10;
                if (_leftScrollAdapter.ItemCount != infoRows) ConfigureScrollAdapters();
            }
            _leftScrollAdapter?.Update();
            _rightScrollAdapter?.Update();
            if (_selectedMainTab == 6 && _rightScrollAdapter != null)
                _friendChatAtBottom = _rightScrollAdapter.ScrollY >= _rightScrollAdapter.ScrollLimit - 2;
            _mainTabScrollAdapter.Update();
        }

        public override void updateKey()
        {
            if (_inputContext == null) return;

            if (_clanDialogMode != ClanInputNone)
            {
                HandleClanDialogInput();
                return;
            }

            if (_showIntrinsicInput)
            {
                HandleIntrinsicInput();
                return;
            }

            // 1. ESC or Back key
            if (GameCanvas.keyAsciiPress == 27 || GameCanvas.keyPressed[12] || GameCanvas.keyPressed[13])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                if (_showIntrinsicList)
                {
                    ExitIntrinsicListView();
                    return;
                }
                if (_showIntrinsicConfirmation)
                {
                    ReturnToIntrinsicMain();
                    return;
                }
                if (_selectedMainTab == 1 && _showInventoryDetail)
                {
                    CloseInventoryDetail();
                    return;
                }
                if (_selectedMainTab == 3 && _clanChatFocused)
                {
                    _clanChatFocused = false;
                    _clanChatField?.setFocus(false);
                    return;
                }
                if (_selectedMainTab == 4 && _functionWorldChatFocused)
                {
                    _functionWorldChatFocused = false;
                    _functionWorldChatField?.setFocus(false);
                    return;
                }
                if (_selectedMainTab == 6 && IsFriendInputFocused())
                {
                    BlurFriendInputs();
                    return;
                }
                Close();
                return;
            }

            // 2. F7 key
            if (GameCanvas.keyAsciiPress == -27 || GameCanvas.keyPressed[17])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                Close();
                return;
            }

            // 3. Hierarchical keyboard navigation between sidebar, subtabs, and rows.
            if (_selectedMainTab == 6 && HandleFriendTextInput()) return;
            if (HandleKeyboardNavigation()) return;

            // Pointer on Close Button
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height))
            {
                GameCanvas.isPointerJustRelease = false;
                Close();
                return;
            }

            // Pointer on Vertical Tab Bar
            if (_mainTabScrollAdapter.UpdateKey(_inputContext, out int clickedTab)
                && clickedTab >= 0 && clickedTab < MainTabCount)
            {
                    CloseInventoryDetail();
                    if (clickedTab != 3)
                    {
                        _clanChatFocused = false;
                        _clanChatField?.setFocus(false);
                    }
                    if (clickedTab != 4)
                    {
                        _functionWorldChatFocused = false;
                        _functionWorldChatField?.setFocus(false);
                    }
                    if (_selectedMainTab == 6 && clickedTab != 6) LeaveFriendTab();
                    _selectedMainTab = clickedTab;
                    if (_selectedMainTab == 0) _selectedSubTab = 0;
                    _showSkillKeyPicker = false;
                    ExitIntrinsicListView(false);
                    _skillFocusArea = SkillFocusList;
                    _selectedPotentialAction = _selectedMainTab == 2 ? GetFirstVisiblePotentialAction() : -1;
                    if (_selectedMainTab == 2) RequestSelectedIntrinsicInfo();
                    if (_selectedMainTab == 3) EnterClanTab();
                    if (_selectedMainTab == 4) EnterFunctionTab();
                    if (_selectedMainTab == 5) EnterDiscipleTab();
                    if (_selectedMainTab == 6) EnterFriendTab();
                    _keyboardFocus = KeyboardFocusMainTabs;
                    GameCanvas.isPointerJustRelease = false;
                    _leftScrollAdapter?.Reset();
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    if (_selectedMainTab == 0)
                        _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                    else if (_selectedMainTab == 2 && _selectedSkillRow >= 0)
                        _leftScrollAdapter?.ScrollToIndex(_selectedSkillRow);
                    return;
            }

            if (_selectedMainTab == 0)
            {
                // Pointer on Sub-Tabs
                if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_subTab0Rect.X, _subTab0Rect.Y, _subTab0Rect.Width, _subTab0Rect.Height))
                {
                    _selectedSubTab = 0;
                    _keyboardFocus = KeyboardFocusContent;
                    GameCanvas.isPointerJustRelease = false;
                    _leftScrollAdapter?.Reset();
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                    return;
                }

                if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_subTab1Rect.X, _subTab1Rect.Y, _subTab1Rect.Width, _subTab1Rect.Height))
                {
                    _selectedSubTab = 1;
                    _keyboardFocus = KeyboardFocusContent;
                    GameCanvas.isPointerJustRelease = false;
                    _leftScrollAdapter?.Reset();
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    return;
                }

                // Left Column Scroll and Click
                if (_selectedSubTab == 0 && _leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int clickedIndex))
                {
                    if (clickedIndex >= 0 && clickedIndex < GetMainTaskRowCount())
                    {
                        _selectedTaskPosition = clickedIndex;
                        _keyboardFocus = KeyboardFocusContent;
                    }
                    _rightScrollAdapter?.Reset();
                    ConfigureScrollAdapters();
                    return;
                }

                // Pointer Click on Left Column (Nhiệm vụ khác)
                if (_selectedSubTab == 1 && GameCanvas.isPointerJustRelease)
                {
                    for (int i = 0; i < _otherQuestCardRects.Length; i++)
                    {
                        UiRect rect = _otherQuestCardRects[i];
                        if (rect.Width > 0 && GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height))
                        {
                            _selectedOtherCategoryIndex = i;
                            _keyboardFocus = KeyboardFocusContent;
                            GameCanvas.isPointerJustRelease = false;
                            _rightScrollAdapter?.Reset();
                            ConfigureScrollAdapters();
                            return;
                        }
                    }
                }

                // Right Column Scroll
                if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out _))
                {
                    return;
                }
            }
            else if (_selectedMainTab == 1 && HandleInventoryPointerInput())
            {
                return;
            }
            else if (_selectedMainTab == 2 && HandleSkillPointerInput())
            {
                return;
            }
            else if (_selectedMainTab == 3 && HandleClanPointerInput())
            {
                return;
            }
            else if (_selectedMainTab == 4 && HandleFunctionPointerInput())
            {
                return;
            }
            else if (_selectedMainTab == 5 && HandleDisciplePointerInput())
            {
                return;
            }
            else if (_selectedMainTab == 6 && HandleFriendPointerInput())
            {
                return;
            }

            base.updateKey();
        }

    }
}

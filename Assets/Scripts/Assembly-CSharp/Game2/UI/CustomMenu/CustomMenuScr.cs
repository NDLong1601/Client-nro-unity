using System;
using System.Collections.Generic;
using Game2.UI.Adapters;
using Game2.UI.Components;
using Nro.UI;

namespace Game2.UI.CustomMenu
{
    public class CustomMenuScr : mScreen
    {
        private static CustomMenuScr _instance;
        private static bool _isOpen;

        private mScreen _previousScreen;
        private UiInputContext _inputContext;

        public static bool IsOpen
        {
            get { return _isOpen; }
        }

        // Tabs state
        private int _selectedMainTab = 0; // 0: Nhiệm vụ, 1: Hành trang, 2: Kỹ năng, 3: Bang hội, 4: Chức năng
        private int _selectedSubTab = 0;  // 0: Nhiệm vụ chính, 1: Nhiệm vụ khác
        private int _selectedTaskPosition = 0;
        private int _selectedOtherCategoryIndex = 0; // 0: Bò Mộng, 1: Kanao, 2: Ngư Dân, 3: Bang Hội
        private int _selectedInventoryLeftTab;
        private int _selectedInventoryBagTab;
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
        private int _keyboardFocus = KeyboardFocusContent;
        private bool _hasInitializedState;

        // Scroll adapters
        private ScrollViewAdapter _leftScrollAdapter;
        private ScrollViewAdapter _rightScrollAdapter;

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

        private const int MainTabCount = 5;
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
        private const int InventoryListRowHeight = 39;
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
            "Chức năng"
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
            if (_instance._rightScrollAdapter != null
                && _instance._rightScrollAdapter.Viewport.Contains(pointerX, pointerY))
            {
                _instance._rightScrollAdapter.ScrollByWheel(wheelDelta);
                return true;
            }
            return _instance._frameRect.Contains(pointerX, pointerY);
        }

        private bool HandleKeyboardNavigation()
        {
            if (GameCanvas.keyPressed[23] || GameCanvas.keyPressed[4])
            {
                ConsumeDirectionKeys(23, 4);
                MoveHorizontalFocus(-1);
                return true;
            }
            if (GameCanvas.keyPressed[24] || GameCanvas.keyPressed[6])
            {
                ConsumeDirectionKeys(24, 6);
                MoveHorizontalFocus(1);
                return true;
            }
            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                MoveVerticalSelection(-1);
                return true;
            }
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                MoveVerticalSelection(1);
                return true;
            }
            if ((_selectedMainTab == 1 || _selectedMainTab == 2 || _selectedMainTab == 3)
                && GameCanvas.keyPressed[Main.isPC ? 25 : 5])
            {
                GameCanvas.keyPressed[25] = false;
                GameCanvas.keyPressed[15] = false;
                GameCanvas.keyPressed[5] = false;
                GameCanvas.keyHold[25] = false;
                GameCanvas.keyHold[15] = false;
                GameCanvas.keyHold[5] = false;
                if (_keyboardFocus == KeyboardFocusMainTabs) MoveHorizontalFocus(1);
                else if (_selectedMainTab == 1) HandleInventoryConfirm();
                else if (_selectedMainTab == 3) HandleClanConfirm();
                else HandleSkillConfirm();
                return true;
            }
            return false;
        }

        private static void ConsumeDirectionKeys(int arrowKey, int legacyKey)
        {
            GameCanvas.keyPressed[arrowKey] = false;
            GameCanvas.keyPressed[legacyKey] = false;
            GameCanvas.keyHold[arrowKey] = false;
            GameCanvas.keyHold[legacyKey] = false;
        }

        private void MoveHorizontalFocus(int direction)
        {
            if (_keyboardFocus == KeyboardFocusMainTabs)
            {
                if (direction <= 0) return;
                _keyboardFocus = KeyboardFocusContent;
                if (_selectedMainTab == 0)
                {
                    _selectedSubTab = 0;
                    RefreshKeyboardPage();
                }
                else if (_selectedMainTab == 1)
                {
                    _inventoryFocusArea = InventoryFocusLeft;
                    if (_selectedInventoryLeftTab == 0 && _selectedInventoryBodySlot < 0)
                        _selectedInventoryBodySlot = EquipmentVisualOrder[0];
                }
                else if (_selectedMainTab == 2 && _selectedSkillRow < 0 && GetSkillRowCount() > 0)
                {
                    _selectedSkillRow = 0;
                    _skillFocusArea = SkillFocusList;
                    _selectedPotentialAction = GetFirstVisiblePotentialAction();
                    RefreshKeyboardPage();
                }
                else if (_selectedMainTab == 3)
                {
                    _clanFocusArea = ClanFocusFunctions;
                    _selectedClanFunction = GetClanFunctionForView(_selectedClanView);
                }
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab == 1)
            {
                if (_inventoryFocusArea == InventoryFocusActions)
                {
                    MoveInventoryActionFocus(direction);
                    return;
                }
                if (_inventoryFocusArea == InventoryFocusBagItems)
                {
                    if (ModFunc.isInventory) MoveInventoryHorizontalSelection(direction);
                    else if (direction < 0)
                    {
                        _inventoryFocusArea = InventoryFocusLeft;
                        SoundMn.gI().panelClick();
                    }
                    return;
                }
                if (_inventoryFocusArea == InventoryFocusBagTabs)
                {
                    if (direction < 0 && _selectedInventoryBagTab > 0) SelectInventoryBagTab(_selectedInventoryBagTab - 1);
                    else if (direction > 0 && _selectedInventoryBagTab < 1) SelectInventoryBagTab(_selectedInventoryBagTab + 1);
                    else if (direction < 0)
                    {
                        _inventoryFocusArea = InventoryFocusLeft;
                        SoundMn.gI().panelClick();
                    }
                    return;
                }
                if (direction < 0)
                {
                    if (_selectedInventoryLeftTab > 0) SelectInventoryLeftTab(_selectedInventoryLeftTab - 1);
                    else
                    {
                        _keyboardFocus = KeyboardFocusMainTabs;
                        SoundMn.gI().panelClick();
                    }
                }
                else if (_selectedInventoryLeftTab < 1) SelectInventoryLeftTab(_selectedInventoryLeftTab + 1);
                else
                {
                    _inventoryFocusArea = InventoryFocusBagTabs;
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (_selectedMainTab == 2)
            {
                if (direction < 0)
                {
                    if (_skillFocusArea == SkillFocusKeys)
                    {
                        _showSkillKeyPicker = false;
                        _skillFocusArea = SkillFocusDetail;
                    }
                    else if (_skillFocusArea == SkillFocusIntrinsicSide)
                    {
                        _skillFocusArea = SkillFocusDetail;
                    }
                    else if (_skillFocusArea == SkillFocusDetail)
                    {
                        _skillFocusArea = SkillFocusList;
                    }
                    else
                    {
                        _keyboardFocus = KeyboardFocusMainTabs;
                    }
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusList)
                {
                    if (_selectedSkillRow >= 0 && _selectedSkillRow < PotentialStatRowCount)
                    {
                        _selectedPotentialAction = GetFirstVisiblePotentialAction();
                        if (_selectedPotentialAction < 0) return;
                        _skillFocusArea = SkillFocusDetail;
                        SoundMn.gI().panelClick();
                    }
                    else if (_selectedSkillRow == IntrinsicRowIndex)
                    {
                        EnsureDefaultIntrinsicActions();
                        if (_intrinsicActionCount <= 0) return;
                        if (_selectedIntrinsicAction < 0) _selectedIntrinsicAction = 0;
                        _skillFocusArea = SkillFocusDetail;
                        SoundMn.gI().panelClick();
                    }
                    else if (_selectedSkillRow >= SkillTemplateStartRow && GetSelectedSkill() != null)
                    {
                        _skillFocusArea = SkillFocusDetail;
                        SoundMn.gI().panelClick();
                    }
                }
                else if (_skillFocusArea == SkillFocusDetail && (_showIntrinsicList || _showIntrinsicConfirmation))
                {
                    _selectedIntrinsicSideAction = _showIntrinsicList && _selectedIntrinsicListIndex >= 0 ? 1 : 0;
                    _skillFocusArea = SkillFocusIntrinsicSide;
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusDetail && _selectedSkillRow >= SkillTemplateStartRow)
                {
                    OpenSkillKeyPicker();
                }
                return;
            }

            if (_selectedMainTab == 3)
            {
                MoveClanHorizontalFocus(direction);
                return;
            }

            if (_selectedMainTab != 0)
            {
                if (direction < 0)
                {
                    _keyboardFocus = KeyboardFocusMainTabs;
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (direction > 0)
            {
                if (_selectedSubTab < 1)
                {
                    _selectedSubTab++;
                    if (!allOtherQuests[_selectedOtherCategoryIndex].hasQuest) SelectFirstAvailableOtherQuest();
                    RefreshKeyboardPage();
                    SoundMn.gI().panelClick();
                }
                return;
            }

            if (_selectedSubTab > 0)
            {
                _selectedSubTab--;
                RefreshKeyboardPage();
            }
            else
            {
                _keyboardFocus = KeyboardFocusMainTabs;
            }
            SoundMn.gI().panelClick();
        }

        private void MoveVerticalSelection(int direction)
        {
            if (_keyboardFocus == KeyboardFocusMainTabs)
            {
                CloseInventoryDetail();
                if (_selectedMainTab == 3)
                {
                    _clanChatFocused = false;
                    _clanChatField?.setFocus(false);
                }
                _selectedMainTab = (_selectedMainTab + direction + MainTabCount) % MainTabCount;
                if (_selectedMainTab == 0) _selectedSubTab = 0;
                _showSkillKeyPicker = false;
                _skillFocusArea = SkillFocusList;
                _selectedPotentialAction = -1;
                _selectedIntrinsicAction = -1;
                if (_selectedMainTab == 3) EnterClanTab();
                RefreshKeyboardPage();
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab == 1)
            {
                if (_inventoryFocusArea == InventoryFocusBagTabs)
                {
                    if (direction > 0) FocusInventoryBagItems();
                }
                else if (_inventoryFocusArea == InventoryFocusBagItems) MoveInventoryVerticalSelection(direction);
                else if (_inventoryFocusArea == InventoryFocusActions) MoveInventoryActionVertical(direction);
                else MoveInventoryLeftSelection(direction);
                return;
            }

            if (_selectedMainTab == 2)
            {
                if (_showIntrinsicList && _skillFocusArea == SkillFocusDetail)
                    MoveIntrinsicListSelection(direction);
                else if (_showIntrinsicList && _skillFocusArea == SkillFocusIntrinsicSide)
                {
                    if (_selectedIntrinsicListIndex < 0) _selectedIntrinsicSideAction = 0;
                    else _selectedIntrinsicSideAction = _selectedIntrinsicSideAction == 0 ? 1 : 0;
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusDetail && _selectedSkillRow < PotentialStatRowCount)
                    MovePotentialAction(direction);
                else if (_skillFocusArea == SkillFocusDetail && _selectedSkillRow == IntrinsicRowIndex)
                    MoveIntrinsicAction(direction);
                else if (_skillFocusArea == SkillFocusKeys)
                {
                    _selectedSkillKeyIndex = (_selectedSkillKeyIndex + direction + SkillKeyButtonCount) % SkillKeyButtonCount;
                    SoundMn.gI().panelClick();
                }
                else if (_skillFocusArea == SkillFocusList)
                    MoveRowSelection(direction);
                return;
            }

            if (_selectedMainTab == 3)
            {
                MoveClanVerticalSelection(direction);
                return;
            }

            MoveRowSelection(direction);
        }

        private void HandleSkillConfirm()
        {
            if (_skillFocusArea == SkillFocusList)
            {
                MoveHorizontalFocus(1);
                return;
            }
            if (_skillFocusArea == SkillFocusKeys)
            {
                AssignSelectedSkillToKey(_selectedSkillKeyIndex);
                return;
            }
            if (_showIntrinsicList && _skillFocusArea == SkillFocusIntrinsicSide)
            {
                if (_selectedIntrinsicSideAction == 0) ExitIntrinsicListView();
                else OpenIntrinsicInput();
                return;
            }
            if (_showIntrinsicConfirmation && _skillFocusArea == SkillFocusIntrinsicSide)
            {
                ReturnToIntrinsicMain();
                return;
            }
            if (_skillFocusArea != SkillFocusDetail) return;
            if (_showIntrinsicList)
            {
                if (_selectedIntrinsicListIndex >= 0)
                {
                    _selectedIntrinsicSideAction = 1;
                    _skillFocusArea = SkillFocusIntrinsicSide;
                    SoundMn.gI().panelClick();
                }
                return;
            }
            if (_selectedSkillRow < PotentialStatRowCount)
            {
                if (!IsPotentialActionVisible(_selectedPotentialAction)) return;
                if (_selectedPotentialAction == 3) OpenAutoPotentialInput();
                else IncreaseSelectedPotential(GetPotentialBatch(_selectedPotentialAction));
                return;
            }
            if (_selectedSkillRow == IntrinsicRowIndex)
            {
                PerformIntrinsicAction(_selectedIntrinsicAction);
                return;
            }
            if (_selectedSkillRow >= SkillTemplateStartRow) OpenSkillKeyPicker();
        }

        private void MovePotentialAction(int direction)
        {
            int start = _selectedPotentialAction;
            for (int step = 1; step <= _potentialButtonRects.Length; step++)
            {
                int candidate = (start + direction * step + _potentialButtonRects.Length * 2) % _potentialButtonRects.Length;
                if (!IsPotentialActionVisible(candidate)) continue;
                _selectedPotentialAction = candidate;
                SoundMn.gI().panelClick();
                return;
            }
        }

        private void MoveIntrinsicAction(int direction)
        {
            EnsureDefaultIntrinsicActions();
            if (_intrinsicActionCount <= 0) return;
            if (_selectedIntrinsicAction < 0) _selectedIntrinsicAction = 0;
            else _selectedIntrinsicAction = (_selectedIntrinsicAction + direction + _intrinsicActionCount) % _intrinsicActionCount;
            SoundMn.gI().panelClick();
        }

        private void MoveIntrinsicListSelection(int direction)
        {
            int count = GetIntrinsicListCount();
            if (count <= 0) return;
            if (_selectedIntrinsicListIndex < 0)
                _selectedIntrinsicListIndex = direction > 0 ? 0 : count - 1;
            else
                _selectedIntrinsicListIndex = System.Math.Max(0,
                    System.Math.Min(count - 1, _selectedIntrinsicListIndex + direction));
            _rightScrollAdapter?.ScrollToIndex(_selectedIntrinsicListIndex);
            SoundMn.gI().panelClick();
        }

        private void RefreshKeyboardPage()
        {
            _leftScrollAdapter?.Reset();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            if (_selectedMainTab == 0 && _selectedSubTab == 0)
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
            else if (_selectedMainTab == 2 && _selectedSkillRow >= 0)
                _leftScrollAdapter?.ScrollToIndex(_selectedSkillRow);
        }

        private void MoveRowSelection(int direction)
        {
            if (_selectedMainTab == 2)
            {
                int rowCount = GetSkillRowCount();
                if (rowCount <= 0) return;
                if (_selectedSkillRow < 0)
                    _selectedSkillRow = direction > 0 ? 0 : rowCount - 1;
                else
                    _selectedSkillRow = (_selectedSkillRow + direction + rowCount) % rowCount;
                _showSkillKeyPicker = false;
                ExitIntrinsicListView(false);
                _skillFocusArea = SkillFocusList;
                _selectedPotentialAction = GetFirstVisiblePotentialAction();
                RequestSelectedIntrinsicInfo();
                _leftScrollAdapter?.ScrollToIndex(_selectedSkillRow);
                SoundMn.gI().panelClick();
                return;
            }

            if (_selectedMainTab != 0) return;

            if (_selectedSubTab == 0)
            {
                int rowCount = GetMainTaskRowCount();
                if (rowCount <= 0) return;
                _selectedTaskPosition = (_selectedTaskPosition + direction + rowCount) % rowCount;
                _rightScrollAdapter?.Reset();
                ConfigureScrollAdapters();
                _leftScrollAdapter?.ScrollToIndex(_selectedTaskPosition);
                SoundMn.gI().panelClick();
                return;
            }

            List<int> visibleQuests = new List<int>();
            for (int i = 0; i < OtherQuestNavigationOrder.Length; i++)
            {
                int questIndex = OtherQuestNavigationOrder[i];
                if (allOtherQuests[questIndex].hasQuest) visibleQuests.Add(questIndex);
            }
            if (visibleQuests.Count == 0) return;

            int currentPosition = visibleQuests.IndexOf(_selectedOtherCategoryIndex);
            int nextPosition = currentPosition < 0
                ? (direction > 0 ? 0 : visibleQuests.Count - 1)
                : (currentPosition + direction + visibleQuests.Count) % visibleQuests.Count;
            _selectedOtherCategoryIndex = visibleQuests[nextPosition];
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();

            UiRect selectedRect = _otherQuestCardRects[_selectedOtherCategoryIndex];
            if (selectedRect.Width > 0 && _leftScrollAdapter != null)
            {
                int contentOffset = selectedRect.Y - _leftColRect.Y + _leftScrollAdapter.ScrollY;
                _leftScrollAdapter.ScrollToIndex(contentOffset / 10);
            }
            SoundMn.gI().panelClick();
        }

        // ----------------------------------------------------
        // Real-Time Server Event Listeners (Hooked in Controller)
        // ----------------------------------------------------
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
            _rightBodyRect = new UiRect(_rightColRect.X + 2, _rightColRect.Y + 26, _rightColRect.Width - 4, _rightColRect.Height - 28);
            ConfigureEquipmentSlotRects();
            ConfigureInventoryDetailRects();
            ConfigureSkillActionRects();
            ConfigureClanRects();

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
                    GameCanvas.loadImage("/custom_menu/tab_task.png"),
                    GameCanvas.loadImage("/custom_menu/tab_inventory.png"),
                    GameCanvas.loadImage("/custom_menu/tab_skill.png"),
                    GameCanvas.loadImage("/custom_menu/tab_clan.png"),
                    GameCanvas.loadImage("/custom_menu/tab_function.png")
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

        private void ConfigureScrollAdapters()
        {
            if (_leftScrollAdapter == null) _leftScrollAdapter = new ScrollViewAdapter();
            if (_rightScrollAdapter == null) _rightScrollAdapter = new ScrollViewAdapter();

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

        private int GetMainTaskRowCount()
        {
            return BuildMainTaskSequence().Length;
        }

        private static int GetInventoryTabStart(int tab, int bagLength)
        {
            int firstTabCount = (bagLength + 1) / 2;
            return tab <= 0 ? 0 : firstTabCount;
        }

        private static int GetInventoryTabItemCount(int tab, int bagLength)
        {
            int start = GetInventoryTabStart(tab, bagLength);
            int end = tab <= 0 ? (bagLength + 1) / 2 : bagLength;
            return System.Math.Max(0, end - start);
        }

        private int GetInventoryBagRowCount()
        {
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int itemCount = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            return ModFunc.isInventory ? (itemCount + InventoryGridColumns - 1) / InventoryGridColumns : itemCount;
        }

        private int GetInventoryInfoContentHeight()
        {
            Char me = Char.myCharz();
            if (me == null) return _leftColRect.Height;
            int lineCount = 0;
            int width = System.Math.Max(40, _leftColRect.Width - 10);
            List<string> lines = BuildPlayerInformationLines(me);
            for (int i = 0; i < lines.Count; i++)
            {
                if (string.IsNullOrEmpty(lines[i])) lineCount++;
                else lineCount += System.Math.Max(1, mFont.tahoma_7b_dark.splitFontArray(lines[i], width).Length);
            }
            return PlayerProfileHeaderHeight + 19 + lineCount * PlayerInfoLineHeight + 6;
        }

        private static int GetSkillRowCount()
        {
            Char me = Char.myCharz();
            int templateCount = me != null && me.nClass != null && me.nClass.skillTemplates != null
                ? me.nClass.skillTemplates.Length
                : 0;
            return SkillTemplateStartRow + templateCount;
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
            if (_selectedMainTab == 1 && _selectedInventoryLeftTab == 1 && _leftScrollAdapter != null)
            {
                int infoRows = (GetInventoryInfoContentHeight() + 9) / 10;
                if (_leftScrollAdapter.ItemCount != infoRows) ConfigureScrollAdapters();
            }
            _leftScrollAdapter?.Update();
            _rightScrollAdapter?.Update();
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
            if (HandleKeyboardNavigation()) return;

            // Pointer on Close Button
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height))
            {
                GameCanvas.isPointerJustRelease = false;
                Close();
                return;
            }

            // Pointer on Vertical Tab Bar
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height))
            {
                int clickedTab = (GameCanvas.py - _tabBarRect.Y) * MainTabCount / _tabBarRect.Height;
                if (clickedTab >= 0 && clickedTab < MainTabCount)
                {
                    CloseInventoryDetail();
                    if (clickedTab != 3)
                    {
                        _clanChatFocused = false;
                        _clanChatField?.setFocus(false);
                    }
                    _selectedMainTab = clickedTab;
                    if (_selectedMainTab == 0) _selectedSubTab = 0;
                    _showSkillKeyPicker = false;
                    ExitIntrinsicListView(false);
                    _skillFocusArea = SkillFocusList;
                    _selectedPotentialAction = _selectedMainTab == 2 ? GetFirstVisiblePotentialAction() : -1;
                    if (_selectedMainTab == 2) RequestSelectedIntrinsicInfo();
                    if (_selectedMainTab == 3) EnterClanTab();
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

            base.updateKey();
        }

        public override void paint(mGraphics g)
        {
            g.translate(-g.getTranslateX(), -g.getTranslateY());
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);

            // 1. Drop shadow for window
            g.setColor(0x000000);
            g.fillRect(_frameRect.X + 3, _frameRect.Y + 3, _frameRect.Width, _frameRect.Height);

            // 2. Main Frame Background
            g.setColor(0xECE0CF); // Warm cream fill
            g.fillRect(_frameRect.X, _frameRect.Y, _frameRect.Width, _frameRect.Height);

            // Frame border
            g.setColor(0x5C4638);
            g.drawRect(_frameRect.X, _frameRect.Y, _frameRect.Width, _frameRect.Height);

            // 3. Left Vertical Tab Bar
            PaintVerticalTabBar(g);

            // 4. Footer Bar
            PaintFooterBar(g);

            // 5. Content Area
            if (_selectedMainTab == 0)
            {
                PaintTaskTabContent(g);
            }
            else if (_selectedMainTab == 1)
            {
                PaintInventoryTabContent(g);
            }
            else if (_selectedMainTab == 2)
            {
                PaintSkillTabContent(g);
            }
            else if (_selectedMainTab == 3)
            {
                PaintClanTabContent(g);
            }
            else
            {
                PaintEmptyTabContent(g);
            }
            if (_showIntrinsicInput) PaintIntrinsicInput(g);
            if (_clanDialogMode != ClanInputNone) PaintClanDialog(g);
        }

        private void PaintVerticalTabBar(mGraphics g)
        {
            g.setColor(0xEAE5DC);
            g.fillRect(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height);
            g.setColor(0x3E3A34);
            g.drawRect(_tabBarRect.X, _tabBarRect.Y, _tabBarRect.Width, _tabBarRect.Height);

            for (int i = 0; i < MainTabCount; i++)
            {
                int ty = _tabBarRect.Y + i * _tabBarRect.Height / MainTabCount;
                int nextY = _tabBarRect.Y + (i + 1) * _tabBarRect.Height / MainTabCount;
                int tabH = nextY - ty;
                bool isSelected = (i == _selectedMainTab);
                if (isSelected)
                {
                    g.setColor(0xFFCB35);
                    g.fillRect(_tabBarRect.X + 1, ty + 1, _tabBarRect.Width - 1, tabH - 1);
                    if (_keyboardFocus == KeyboardFocusMainTabs)
                    {
                        g.setColor(0xFFF2A8);
                        g.drawRect(_tabBarRect.X + 2, ty + 2, _tabBarRect.Width - 5, tabH - 5);
                    }
                }
                g.setColor(0x766B5D);
                g.drawLine(_tabBarRect.X, ty + tabH, _tabBarRect.X + _tabBarRect.Width, ty + tabH);
                Image icon = _mainTabIcons != null && i < _mainTabIcons.Length ? _mainTabIcons[i] : null;
                if (icon != null)
                {
                    g.drawImage(icon, _tabBarRect.X + _tabBarRect.Width / 2, ty + tabH / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                }
            }
        }

        private void PaintFooterBar(mGraphics g)
        {
            g.setColor(0xE9E4DA);
            g.fillRect(_footerRect.X, _footerRect.Y, _footerRect.Width, _footerRect.Height);
            g.setColor(0x7B6F60);
            g.drawRect(_footerRect.X, _footerRect.Y, _footerRect.Width, _footerRect.Height);

            Char me = Char.myCharz();
            long xu = me != null ? me.xu : 0L;
            int luong = me != null ? me.luong : 0;
            int luongKhoa = me != null ? me.luongKhoa : 0;
            int usableW = _footerRect.Width - 28;
            PaintCurrency(g, _footerRect.X + usableW / 6, Panel.imgXu, NinjaUtil.getMoneys(xu));
            PaintCurrency(g, _footerRect.X + usableW / 2, Panel.imgLuong, NinjaUtil.getMoneys(luong));
            PaintCurrency(g, _footerRect.X + usableW * 5 / 6, Panel.imgLuongKhoa, NinjaUtil.getMoneys(luongKhoa));

            g.setColor(0xF7C400);
            g.fillRect(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height);
            g.setColor(0xE59600);
            g.drawRect(_closeBtnRect.X, _closeBtnRect.Y, _closeBtnRect.Width, _closeBtnRect.Height);
            g.setColor(0xE35A24);
            g.drawLine(_closeBtnRect.X + 5, _closeBtnRect.Y + 5, _closeBtnRect.X + 17, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 6, _closeBtnRect.Y + 5, _closeBtnRect.X + 18, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 17, _closeBtnRect.Y + 5, _closeBtnRect.X + 5, _closeBtnRect.Y + 17);
            g.drawLine(_closeBtnRect.X + 18, _closeBtnRect.Y + 5, _closeBtnRect.X + 6, _closeBtnRect.Y + 17);
        }

        private void PaintCurrency(mGraphics g, int centerX, Image icon, string value)
        {
            int textW = mFont.tahoma_7_orange.getWidth(value);
            int iconW = icon != null ? icon.getWidth() : 14;
            int startX = centerX - (iconW + 5 + textW) / 2;
            if (icon != null) g.drawImage(icon, startX + iconW / 2, _footerRect.Y + _footerRect.Height / 2, mGraphics.HCENTER | mGraphics.VCENTER);
            mFont.tahoma_7_orange.drawString(g, value, startX + iconW + 5, _footerRect.Y + 7, mFont.LEFT);
        }

        private void PaintEmptyTabContent(mGraphics g)
        {
            int midX = _contentRect.X + _contentRect.Width / 2;
            int midY = _contentRect.Y + _contentRect.Height / 2;

            g.setColor(0xDFD2BC);
            g.fillRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);
            g.setColor(0xC4B79B);
            g.drawRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);

            string tabName = MainTabNames[_selectedMainTab];
            mFont.tahoma_7b_dark.drawString(g, tabName, midX, midY - 14, mFont.CENTER);
            mFont.tahoma_7_grey.drawString(g, "Chức năng đang được phát triển...", midX, midY + 4, mFont.CENTER);
        }

        private void ConfigureEquipmentSlotRects()
        {
            const int gap = 2;
            const int columns = 5;
            const int rows = 5;
            int cellWidth = System.Math.Max(20, (_leftColRect.Width - gap * (columns + 1)) / columns);
            int cellHeight = System.Math.Max(20, (_leftColRect.Height - gap * (rows + 1)) / rows);
            int gridWidth = cellWidth * columns + gap * (columns - 1);
            int gridHeight = cellHeight * rows + gap * (rows - 1);
            int gridX = _leftColRect.X + (_leftColRect.Width - gridWidth) / 2;
            int gridY = _leftColRect.Y + (_leftColRect.Height - gridHeight) / 2;
            int rightX = gridX + (columns - 1) * (cellWidth + gap);

            for (int row = 0; row < 3; row++)
            {
                int y = gridY + row * (cellHeight + gap);
                _equipmentSlotRects[row] = new UiRect(gridX, y, cellWidth, cellHeight);
                _equipmentSlotRects[row + 3] = new UiRect(rightX, y, cellWidth, cellHeight);
            }

            for (int index = 0; index < 10; index++)
            {
                int column = index % columns;
                int row = index / columns + 3;
                int x = gridX + column * (cellWidth + gap);
                int y = gridY + row * (cellHeight + gap);
                _equipmentSlotRects[index + 6] = new UiRect(x, y, cellWidth, cellHeight);
            }
        }

        private void ConfigureInventoryDetailRects()
        {
            _inventoryDetailRect = UiRect.Empty;
            for (int i = 0; i < _inventoryActionRects.Length; i++) _inventoryActionRects[i] = UiRect.Empty;
            if (!_showInventoryDetail) return;

            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            if (item == null || item.template == null) return;

            const int gap = 3;
            const int buttonHeight = 26;
            if (fromBody)
            {
                int detailHeight = System.Math.Min(126, _leftColRect.Height - 8);
                _inventoryDetailRect = new UiRect(_leftColRect.X + 2, _leftColRect.Bottom - detailHeight,
                    _leftColRect.Width - 4, detailHeight);
                int buttonWidth = System.Math.Max(48, (_rightBodyRect.Width - gap * 3) / 2);
                int buttonX = _rightBodyRect.X + gap;
                int buttonY = _inventoryDetailRect.Y;
                _inventoryActionRects[0] = new UiRect(buttonX, buttonY, buttonWidth, buttonHeight);
                _inventoryActionRects[1] = new UiRect(buttonX, buttonY + buttonHeight + gap, buttonWidth, buttonHeight);
                return;
            }

            int detailInset = ModFunc.isInventory
                ? System.Math.Max(28, (_rightBodyRect.Width - 12) / InventoryGridColumns + gap)
                : 31;
            int actionAreaHeight = buttonHeight * 2 + gap * 3;
            int detailHeightBag = System.Math.Min(112, System.Math.Max(72, _rightBodyRect.Height - actionAreaHeight));
            _inventoryDetailRect = new UiRect(_rightBodyRect.X + detailInset, _rightBodyRect.Y,
                _rightBodyRect.Width - detailInset, detailHeightBag);

            int actionY = _inventoryDetailRect.Bottom + gap;
            int actionWidth = (_rightBodyRect.Width - gap * 3) / 2;
            for (int i = 0; i < _inventoryActionRects.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                _inventoryActionRects[i] = new UiRect(_rightBodyRect.X + gap + column * (actionWidth + gap),
                    actionY + row * (buttonHeight + gap), actionWidth, buttonHeight);
            }
        }

        private void ConfigureSkillActionRects()
        {
            int margin = 7;
            int gap = 6;
            int buttonHeight = 29;
            int buttonWidth = (_rightBodyRect.Width - margin * 2 - gap) / 2;
            int secondRowY = _rightBodyRect.Y + _rightBodyRect.Height - margin - buttonHeight;
            int firstRowY = secondRowY - gap - buttonHeight;
            int leftX = _rightBodyRect.X + margin;
            int rightX = leftX + buttonWidth + gap;

            _potentialButtonRects[0] = new UiRect(leftX, firstRowY, buttonWidth, buttonHeight);
            _potentialButtonRects[1] = new UiRect(rightX, firstRowY, buttonWidth, buttonHeight);
            _potentialButtonRects[2] = new UiRect(leftX, secondRowY, buttonWidth, buttonHeight);
            _potentialButtonRects[3] = new UiRect(rightX, secondRowY, buttonWidth, buttonHeight);
            _assignSkillButtonRect = new UiRect(leftX, secondRowY, _rightBodyRect.Width - margin * 2, buttonHeight);

            int keyGap = 2;
            int keyWidth = 70;
            int keyHeight = System.Math.Max(22, (_frameRect.Height - 4 - keyGap * (SkillKeyButtonCount - 1)) / SkillKeyButtonCount);
            int keyX = _frameRect.X + _frameRect.Width + 4;
            if (keyX + keyWidth > GameCanvas.w - 2) keyX = System.Math.Max(2, GameCanvas.w - keyWidth - 2);
            int keyY = _frameRect.Y + 2;
            for (int i = 0; i < SkillKeyButtonCount; i++)
                _skillKeyButtonRects[i] = new UiRect(keyX, keyY + i * (keyHeight + keyGap), keyWidth, keyHeight);

            int sideHeight = 28;
            _intrinsicSideButtonRects[0] = new UiRect(keyX, keyY, keyWidth, sideHeight);
            _intrinsicSideButtonRects[1] = new UiRect(keyX, keyY + sideHeight + 4, keyWidth, sideHeight);

            int dialogWidth = System.Math.Min(290, GameCanvas.w - 20);
            int dialogHeight = 112;
            int dialogX = (GameCanvas.w - dialogWidth) / 2;
            int dialogY = (GameCanvas.h - dialogHeight) / 2;
            _intrinsicInputDialogRect = new UiRect(dialogX, dialogY, dialogWidth, dialogHeight);
            _intrinsicInputCloseRect = new UiRect(dialogX + dialogWidth - 19, dialogY - 4, 20, 20);
            int inputWidth = dialogWidth - 54;
            _intrinsicNormalButtonRect = new UiRect(dialogX + 25, dialogY + dialogHeight - 34, 78, 27);
            _intrinsicVipButtonRect = new UiRect(dialogX + dialogWidth - 103, dialogY + dialogHeight - 34, 78, 27);
            if (_intrinsicInputField != null)
            {
                _intrinsicInputField.x = dialogX + 27;
                _intrinsicInputField.y = dialogY + 29;
                _intrinsicInputField.width = inputWidth;
                _intrinsicInputField.height = 27;
            }
        }

        private bool HandleInventoryPointerInput()
        {
            ConfigureInventoryDetailRects();
            Item selectedItem = GetSelectedInventoryItem(out bool selectedFromBody, out _);
            int actionCount = GetInventoryActionCount(selectedItem, selectedFromBody);
            if (_showInventoryDetail && selectedItem != null && GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < actionCount; i++)
                {
                    UiRect actionRect = _inventoryActionRects[i];
                    if (!GameCanvas.isPointer(actionRect.X, actionRect.Y, actionRect.Width, actionRect.Height)) continue;
                    _keyboardFocus = KeyboardFocusContent;
                    _inventoryFocusArea = InventoryFocusActions;
                    _selectedInventoryAction = i;
                    GameCanvas.isPointerJustRelease = false;
                    PerformInventoryAction(i);
                    return true;
                }
                if (_inventoryDetailRect.Width > 0 && GameCanvas.isPointer(_inventoryDetailRect.X, _inventoryDetailRect.Y,
                    _inventoryDetailRect.Width, _inventoryDetailRect.Height))
                {
                    GameCanvas.isPointerJustRelease = false;
                    return true;
                }
            }

            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryLeftTab0Rect.X, _inventoryLeftTab0Rect.Y,
                _inventoryLeftTab0Rect.Width, _inventoryLeftTab0Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusLeft;
                SelectInventoryLeftTab(0);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryLeftTab1Rect.X, _inventoryLeftTab1Rect.Y,
                _inventoryLeftTab1Rect.Width, _inventoryLeftTab1Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusLeft;
                SelectInventoryLeftTab(1);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryBagTab0Rect.X, _inventoryBagTab0Rect.Y,
                _inventoryBagTab0Rect.Width, _inventoryBagTab0Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusBagTabs;
                SelectInventoryBagTab(0);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            if (GameCanvas.isPointerJustRelease && GameCanvas.isPointer(_inventoryBagTab1Rect.X, _inventoryBagTab1Rect.Y,
                _inventoryBagTab1Rect.Width, _inventoryBagTab1Rect.Height))
            {
                _keyboardFocus = KeyboardFocusContent;
                _inventoryFocusArea = InventoryFocusBagTabs;
                SelectInventoryBagTab(1);
                GameCanvas.isPointerJustRelease = false;
                return true;
            }

            if (_selectedInventoryLeftTab == 0 && GameCanvas.isPointerJustRelease)
            {
                for (int visualIndex = 0; visualIndex < _equipmentSlotRects.Length; visualIndex++)
                {
                    UiRect rect = _equipmentSlotRects[visualIndex];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    SelectInventoryBodyItem(EquipmentVisualOrder[visualIndex]);
                    GameCanvas.isPointerJustRelease = false;
                    return true;
                }
            }
            else if (_selectedInventoryLeftTab == 1 && _leftScrollAdapter != null)
            {
                if (_leftScrollAdapter.UpdateKey(_inputContext, out _)) return true;
                if (_leftScrollAdapter.IsDragging) return true;
            }

            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int clickedRow))
            {
                SelectInventoryBagItem(clickedRow);
                return true;
            }
            return _rightScrollAdapter != null && _rightScrollAdapter.IsDragging;
        }

        private void SelectInventoryLeftTab(int tab)
        {
            if (tab < 0) tab = 0;
            if (tab > 1) tab = 1;
            if (_selectedInventoryLeftTab == tab)
            {
                CloseInventoryDetail();
                return;
            }
            CloseInventoryDetail();
            _selectedInventoryLeftTab = tab;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
            _leftScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void SelectInventoryBagTab(int tab)
        {
            if (tab < 0) tab = 0;
            if (tab > 1) tab = 1;
            if (_selectedInventoryBagTab == tab)
            {
                CloseInventoryDetail();
                return;
            }
            CloseInventoryDetail();
            _selectedInventoryBagTab = tab;
            _selectedInventoryBagSlot = -1;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryLeftSelection(int direction)
        {
            if (_selectedInventoryLeftTab != 0) return;
            CloseInventoryDetail();
            int visualIndex = System.Array.IndexOf(EquipmentVisualOrder, _selectedInventoryBodySlot);
            if (visualIndex < 0) visualIndex = direction > 0 ? 0 : InventoryEquipmentSlotCount - 1;
            else visualIndex = (visualIndex + direction + InventoryEquipmentSlotCount) % InventoryEquipmentSlotCount;
            _selectedInventoryBodySlot = EquipmentVisualOrder[visualIndex];
            _selectedInventoryBagSlot = -1;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryHorizontalSelection(int direction)
        {
            CloseInventoryDetail();
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (count <= 0) return;
            int localIndex = _selectedInventoryBagSlot - start;
            if (localIndex < 0 || localIndex >= count)
            {
                localIndex = direction > 0 ? 0 : count - 1;
            }
            else
            {
                int currentRow = localIndex / InventoryGridColumns;
                int currentColumn = localIndex % InventoryGridColumns;
                int targetColumn = currentColumn + direction;
                if (targetColumn < 0 || targetColumn >= InventoryGridColumns) return;
                int target = localIndex + direction;
                int targetRow = target / InventoryGridColumns;
                if (target < 0 || target >= count || targetRow != currentRow) return;
                localIndex = target;
            }
            _selectedInventoryBagSlot = start + localIndex;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            _rightScrollAdapter?.ScrollToIndex(localIndex / InventoryGridColumns);
            ConfigureInventoryDetailRects();
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryVerticalSelection(int direction)
        {
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (count <= 0)
            {
                if (direction < 0) FocusInventoryBagTabs();
                return;
            }

            int localIndex = _selectedInventoryBagSlot - start;
            if (localIndex < 0 || localIndex >= count)
            {
                if (direction < 0) FocusInventoryBagTabs();
                else SetInventoryBagSelection(start, 0);
                return;
            }

            bool isFirstRow = ModFunc.isInventory
                ? localIndex < InventoryGridColumns
                : !ModFunc.isInventory && localIndex == 0;
            if (direction < 0 && isFirstRow)
            {
                FocusInventoryBagTabs();
                return;
            }

            int step = ModFunc.isInventory ? InventoryGridColumns : 1;
            int target = localIndex + direction * step;
            if (target < 0 || target >= count) return;
            SetInventoryBagSelection(start, target);
        }

        private void SetInventoryBagSelection(int start, int localIndex)
        {
            CloseInventoryDetail();
            _selectedInventoryBagSlot = start + localIndex;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            _inventoryFocusArea = InventoryFocusBagItems;
            _rightScrollAdapter?.ScrollToIndex(ModFunc.isInventory ? localIndex / InventoryGridColumns : localIndex);
            ConfigureInventoryDetailRects();
            SoundMn.gI().panelClick();
        }

        private void FocusInventoryBagTabs()
        {
            CloseInventoryDetail();
            _inventoryFocusArea = InventoryFocusBagTabs;
            SoundMn.gI().panelClick();
        }

        private void FocusInventoryBagItems()
        {
            CloseInventoryDetail();
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (count <= 0) return;
            int localIndex = _selectedInventoryBagSlot - start;
            if (localIndex < 0 || localIndex >= count) localIndex = 0;
            SetInventoryBagSelection(start, localIndex);
        }

        private void SelectInventoryBagItem(int clickedRow)
        {
            Char me = Char.myCharz();
            int bagLength = me != null && me.arrItemBag != null ? me.arrItemBag.Length : 0;
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bagLength);
            int localIndex = clickedRow;
            if (ModFunc.isInventory)
            {
                int gap = 2;
                int cellWidth = System.Math.Max(1, (_rightBodyRect.Width - gap * (InventoryGridColumns + 1)) / InventoryGridColumns);
                int column = (GameCanvas.px - _rightBodyRect.X - gap) / System.Math.Max(1, cellWidth + gap);
                if (column < 0) column = 0;
                if (column >= InventoryGridColumns) column = InventoryGridColumns - 1;
                localIndex = clickedRow * InventoryGridColumns + column;
            }
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bagLength);
            if (localIndex < 0 || localIndex >= count) return;
            int bagIndex = start + localIndex;
            bool openDetail = _selectedInventoryBagSlot == bagIndex && _selectedInventoryBodySlot < 0;
            CloseInventoryDetail();
            _selectedInventoryBagSlot = bagIndex;
            _selectedInventoryBodySlot = -1;
            _selectedInventoryAction = 0;
            _inventoryFocusArea = InventoryFocusBagItems;
            if (openDetail) OpenInventoryDetail();
            SoundMn.gI().panelClick();
        }

        private void SelectInventoryBodyItem(int slot)
        {
            bool openDetail = _selectedInventoryBodySlot == slot && _selectedInventoryBagSlot < 0;
            CloseInventoryDetail();
            _selectedInventoryBodySlot = slot;
            _selectedInventoryBagSlot = -1;
            _selectedInventoryAction = 0;
            _inventoryFocusArea = InventoryFocusLeft;
            if (openDetail) OpenInventoryDetail();
            SoundMn.gI().panelClick();
        }

        private void OpenInventoryDetail()
        {
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            if (GetInventoryActionCount(item, fromBody) <= 0) return;
            _showInventoryDetail = true;
            _inventoryFocusArea = InventoryFocusActions;
            _selectedInventoryAction = 0;
            ConfigureInventoryDetailRects();
        }

        private void CloseInventoryDetail()
        {
            _showInventoryDetail = false;
            _inventoryDetailRect = UiRect.Empty;
            for (int i = 0; i < _inventoryActionRects.Length; i++) _inventoryActionRects[i] = UiRect.Empty;
            if (_inventoryFocusArea == InventoryFocusActions)
                _inventoryFocusArea = _selectedInventoryBodySlot >= 0 ? InventoryFocusLeft : InventoryFocusBagItems;
        }

        private void HandleInventoryConfirm()
        {
            if (_inventoryFocusArea == InventoryFocusBagTabs)
            {
                FocusInventoryBagItems();
                return;
            }
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            int actionCount = GetInventoryActionCount(item, fromBody);
            if (actionCount <= 0) return;
            if (!_showInventoryDetail || _inventoryFocusArea != InventoryFocusActions)
            {
                OpenInventoryDetail();
                SoundMn.gI().panelClick();
                return;
            }
            PerformInventoryAction(_selectedInventoryAction);
        }

        private void MoveInventoryActionFocus(int direction)
        {
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            int count = GetInventoryActionCount(item, fromBody);
            if (count <= 0) return;
            int target = _selectedInventoryAction + direction;
            if (fromBody || target < 0 || target >= count || target / 2 != _selectedInventoryAction / 2) return;
            _selectedInventoryAction = target;
            SoundMn.gI().panelClick();
        }

        private void MoveInventoryActionVertical(int direction)
        {
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            int count = GetInventoryActionCount(item, fromBody);
            if (count <= 0) return;
            int step = fromBody ? 1 : 2;
            int target = _selectedInventoryAction + direction * step;
            if (target < 0 || target >= count) return;
            _selectedInventoryAction = target;
            SoundMn.gI().panelClick();
        }

        private Item GetSelectedInventoryItem(out bool fromBody, out int index)
        {
            fromBody = false;
            index = -1;
            Char me = Char.myCharz();
            if (me == null) return null;
            if (_selectedInventoryBodySlot >= 0)
            {
                fromBody = true;
                index = _selectedInventoryBodySlot;
                return me.arrItemBody != null && index < me.arrItemBody.Length ? me.arrItemBody[index] : null;
            }
            index = _selectedInventoryBagSlot;
            return me.arrItemBag != null && index >= 0 && index < me.arrItemBag.Length ? me.arrItemBag[index] : null;
        }

        private static bool CanUseAutoItem(Item item)
        {
            return item != null && item.template != null
                && (item.template.type == 29 || item.template.type == 33 || item.template.id == 380 || item.quantity >= 2);
        }

        private static bool IsAutoItem(Item item)
        {
            return item != null && item.template != null && ModFunc.GI().listItemAuto.Exists(
                autoItem => autoItem.id == item.template.id);
        }

        private static int GetInventoryActionCount(Item item, bool fromBody)
        {
            if (item == null || item.template == null) return 0;
            if (fromBody) return 2;
            return CanUseAutoItem(item) ? 4 : 3;
        }

        private static string GetInventoryActionLabel(Item item, bool fromBody, int action)
        {
            if (fromBody)
            {
                if (action == 0) return "Lấy ra";
                if (action == 1) return "Bỏ ra";
                return string.Empty;
            }
            if (action == 0) return "Sử dụng";
            if (action == 1) return "Sử dụng\ncho đệ tử";
            if (action == 2) return "Bỏ ra";
            if (action == 3) return IsAutoItem(item) ? "Xóa Auto Item" : "Auto Item";
            return string.Empty;
        }

        private void PerformInventoryAction(int action)
        {
            Char me = Char.myCharz();
            if (me == null || me.statusMe == 14)
            {
                GameCanvas.startOKDlg(mResources.can_not_do_when_die);
                return;
            }
            Item item = GetSelectedInventoryItem(out bool fromBody, out int index);
            int actionCount = GetInventoryActionCount(item, fromBody);
            if (item == null || index < 0 || action < 0 || action >= actionCount) return;
            bool actionPerformed = false;

            if (fromBody)
            {
                if (action == 0)
                {
                    Service.gI().getItem(InventoryBodyToBag, (sbyte)index);
                    actionPerformed = true;
                }
                else if (action == 1)
                {
                    Service.gI().useItem(1, InventoryWhereBody, (sbyte)index, -1);
                    actionPerformed = true;
                }
            }
            else if (action == 0)
            {
                if (item.isTypeBody()) Service.gI().getItem(InventoryBagToBody, (sbyte)index);
                else Service.gI().useItem(0, InventoryWhereBag, (sbyte)index, -1);
                actionPerformed = true;
                if (item.template.id == 193 || item.template.id == 194) Close();
            }
            else if (action == 1)
            {
                if (me.havePet)
                {
                    Service.gI().getItem(InventoryBagToPet, (sbyte)index);
                    actionPerformed = true;
                }
                else if (me.havePet2)
                {
                    Service.gI().getItem(InventoryBagToPet2, (sbyte)index);
                    actionPerformed = true;
                }
                else GameScr.info1.addInfo("Bạn chưa có đệ tử.", 0);
            }
            else if (action == 2)
            {
                Service.gI().useItem(1, InventoryWhereBag, (sbyte)index, -1);
                actionPerformed = true;
            }
            else if (action == 3)
            {
                ModFunc.GI().perform(IsAutoItem(item) ? 501 : 500, item);
                actionPerformed = true;
            }
            if (actionPerformed) CloseInventoryDetail();
            SoundMn.gI().panelClick();
        }

        private bool HandleSkillPointerInput()
        {
            if ((_showIntrinsicList || _showIntrinsicConfirmation) && GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _intrinsicSideButtonRects.Length; i++)
                {
                    if (i == 1 && (!_showIntrinsicList || _selectedIntrinsicListIndex < 0)) continue;
                    UiRect sideRect = _intrinsicSideButtonRects[i];
                    if (!GameCanvas.isPointer(sideRect.X, sideRect.Y, sideRect.Width, sideRect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedIntrinsicSideAction = i;
                    _skillFocusArea = SkillFocusIntrinsicSide;
                    if (i == 0 && _showIntrinsicConfirmation) ReturnToIntrinsicMain();
                    else if (i == 0) ExitIntrinsicListView();
                    else OpenIntrinsicInput();
                    return true;
                }
            }

            if (_showIntrinsicList && _rightScrollAdapter != null
                && _rightScrollAdapter.UpdateKey(_inputContext, out int intrinsicIndex))
            {
                if (intrinsicIndex >= 0 && intrinsicIndex < GetIntrinsicListCount())
                {
                    _selectedIntrinsicListIndex = intrinsicIndex;
                    _skillFocusArea = SkillFocusDetail;
                    _selectedIntrinsicSideAction = 1;
                    SoundMn.gI().panelClick();
                }
                return true;
            }

            if (_showSkillKeyPicker && GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _skillKeyButtonRects.Length; i++)
                {
                    UiRect rect = _skillKeyButtonRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedSkillKeyIndex = i;
                    AssignSelectedSkillToKey(i);
                    return true;
                }
            }

            if (_leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int clickedIndex))
            {
                if (clickedIndex >= 0 && clickedIndex < GetSkillRowCount())
                {
                    _selectedSkillRow = clickedIndex;
                    _showSkillKeyPicker = false;
                    ExitIntrinsicListView(false);
                    _skillFocusArea = SkillFocusList;
                    _selectedPotentialAction = GetFirstVisiblePotentialAction();
                    RequestSelectedIntrinsicInfo();
                    _keyboardFocus = KeyboardFocusContent;
                    SoundMn.gI().panelClick();
                }
                return true;
            }

            if (GameCanvas.isPointerJustRelease && _selectedSkillRow >= 0 && _selectedSkillRow < PotentialStatRowCount)
            {
                for (int i = 0; i < _potentialButtonRects.Length; i++)
                {
                    if (!IsPotentialActionVisible(i)) continue;
                    UiRect rect = _potentialButtonRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedPotentialAction = i;
                    _skillFocusArea = SkillFocusDetail;
                    if (i == 3) OpenAutoPotentialInput();
                    else IncreaseSelectedPotential(GetPotentialBatch(i));
                    return true;
                }
            }

            if (GameCanvas.isPointerJustRelease && _selectedSkillRow == IntrinsicRowIndex)
            {
                EnsureDefaultIntrinsicActions();
                for (int i = 0; i < _intrinsicActionCount; i++)
                {
                    UiRect rect = GetIntrinsicActionRect(i);
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.isPointerJustRelease = false;
                    _selectedIntrinsicAction = i;
                    _skillFocusArea = SkillFocusDetail;
                    PerformIntrinsicAction(i);
                    return true;
                }
            }

            if (GameCanvas.isPointerJustRelease && _selectedSkillRow >= SkillTemplateStartRow
                && GameCanvas.isPointer(_assignSkillButtonRect.X, _assignSkillButtonRect.Y, _assignSkillButtonRect.Width, _assignSkillButtonRect.Height))
            {
                GameCanvas.isPointerJustRelease = false;
                _skillFocusArea = SkillFocusDetail;
                OpenSkillKeyPicker();
                return true;
            }

            if (_showSkillKeyPicker && GameCanvas.isPointerJustRelease)
            {
                _showSkillKeyPicker = false;
                _skillFocusArea = SkillFocusDetail;
                GameCanvas.isPointerJustRelease = false;
                return true;
            }
            return false;
        }

        private static long GetPotentialIncreaseValue(Char me, int row)
        {
            if (me == null) return 0L;
            if (row == 0) return me.hpFrom1000TiemNang;
            if (row == 1) return me.mpFrom1000TiemNang;
            if (row == 2) return me.damFrom1000TiemNang;
            if (row == 3) return me.defFrom1000TiemNang;
            if (row == 4) return me.criticalFrom1000Tiemnang;
            return 0L;
        }

        private void RequestSelectedIntrinsicInfo()
        {
            if (_selectedSkillRow != IntrinsicRowIndex)
            {
                ExitIntrinsicListView(false);
                _showIntrinsicConfirmation = false;
                _expectingIntrinsicConfirmation = false;
                return;
            }
            _showIntrinsicConfirmation = false;
            _expectingIntrinsicConfirmation = false;
            _waitingIntrinsicMenu = true;
            _selectedIntrinsicAction = -1;
            EnsureDefaultIntrinsicActions();
            Service.gI().speacialSkill(0);
        }

        private void EnsureDefaultIntrinsicActions()
        {
            if (_intrinsicActionCount > 0 || string.IsNullOrEmpty(Panel.specialInfo)) return;
            _intrinsicActionCount = DefaultIntrinsicActionLabels.Length;
            for (int i = 0; i < _intrinsicActionCount; i++)
            {
                _intrinsicActionLabels[i] = DefaultIntrinsicActionLabels[i];
                _intrinsicActionServerIndices[i] = DefaultIntrinsicActionServerIndices[i];
            }
        }

        private bool CaptureIntrinsicDialog(string text)
        {
            if (!_waitingIntrinsicMenu || _selectedSkillRow != IntrinsicRowIndex) return false;
            _waitingIntrinsicMenu = false;
            _intrinsicDialogText = text ?? string.Empty;
            _intrinsicActionCount = 0;
            string listLabel = null;
            int listServerIndex = -1;

            MyVector menuItems = GameCanvas.menu != null ? GameCanvas.menu.menuItems : null;
            if (menuItems != null)
            {
                int count = System.Math.Min(MaxIntrinsicActionCount, menuItems.size());
                for (int i = 0; i < count; i++)
                {
                    Command command = menuItems.elementAt(i) as Command;
                    if (command == null || string.IsNullOrEmpty(command.caption)) continue;
                    string compact = command.caption.Replace("\r", " ").Replace("\n", " ").Trim();
                    if (compact.IndexOf("Từ chối", StringComparison.OrdinalIgnoreCase) >= 0) continue;
                    if (IsIntrinsicListAction(compact))
                    {
                        listLabel = "Danh sách nội tại";
                        listServerIndex = i;
                        continue;
                    }
                    if (_intrinsicActionCount >= _intrinsicActionLabels.Length - 1) continue;
                    _intrinsicActionLabels[_intrinsicActionCount] = FormatIntrinsicActionLabel(compact);
                    _intrinsicActionServerIndices[_intrinsicActionCount] = i;
                    _intrinsicActionCount++;
                }
            }
            if (listServerIndex >= 0 && _intrinsicActionCount < _intrinsicActionLabels.Length)
            {
                _intrinsicActionLabels[_intrinsicActionCount] = listLabel;
                _intrinsicActionServerIndices[_intrinsicActionCount] = listServerIndex;
                _intrinsicActionCount++;
            }
            EnsureDefaultIntrinsicActions();
            _showIntrinsicConfirmation = _expectingIntrinsicConfirmation && _intrinsicActionCount == 1;
            _expectingIntrinsicConfirmation = false;
            _selectedIntrinsicAction = _intrinsicActionCount > 0 ? 0 : -1;

            if (Char.chatPopup != null)
            {
                Effect2.vEffect2.removeElement(Char.chatPopup);
                Char.chatPopup = null;
            }
            GameCanvas.menu?.doCloseMenu();
            return true;
        }

        private static string FormatIntrinsicActionLabel(string label)
        {
            string compact = label.Replace("\r", " ").Replace("\n", " ").Trim();
            if (IsIntrinsicListAction(compact)) return "Danh sách nội tại";
            return compact;
        }

        private static bool IsIntrinsicListAction(string label)
        {
            return label.IndexOf("Xem tất cả", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("Danh sách", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void PerformIntrinsicAction(int actionIndex)
        {
            if (ModFunc.GI().IsAutoIntrinsicRunning)
            {
                GameScr.info1.addInfo("Đang tự động mở nội tại.", 0);
                return;
            }
            EnsureDefaultIntrinsicActions();
            if (actionIndex < 0 || actionIndex >= _intrinsicActionCount) return;
            string label = _intrinsicActionLabels[actionIndex] ?? string.Empty;
            int serverIndex = _intrinsicActionServerIndices[actionIndex];
            if (serverIndex < 0) return;
            if (_showIntrinsicConfirmation)
            {
                Service.gI().confirmMenu(IntrinsicNpcId, (sbyte)serverIndex);
                _showIntrinsicConfirmation = false;
                _waitingIntrinsicMenu = true;
                _expectingIntrinsicConfirmation = false;
                Service.gI().speacialSkill(0);
                SoundMn.gI().panelClick();
                return;
            }
            bool opensIntrinsicList = IsIntrinsicListAction(label);
            if (opensIntrinsicList)
            {
                _waitingIntrinsicList = true;
                _selectedIntrinsicListIndex = -1;
            }
            else
            {
                _waitingIntrinsicMenu = true;
                _expectingIntrinsicConfirmation = true;
            }
            Service.gI().confirmMenu(IntrinsicNpcId, (sbyte)serverIndex);
            if (opensIntrinsicList && HasIntrinsicListData()) EnterIntrinsicListView();
            SoundMn.gI().panelClick();
        }

        private void ReturnToIntrinsicMain()
        {
            _showIntrinsicConfirmation = false;
            _selectedIntrinsicSideAction = 0;
            _skillFocusArea = SkillFocusDetail;
            _waitingIntrinsicMenu = true;
            _expectingIntrinsicConfirmation = false;
            Service.gI().speacialSkill(0);
            SoundMn.gI().panelClick();
        }

        private UiRect GetIntrinsicActionRect(int actionIndex)
        {
            if (_showIntrinsicConfirmation) return _assignSkillButtonRect;
            return actionIndex == _intrinsicActionCount - 1 && IsIntrinsicListAction(_intrinsicActionLabels[actionIndex] ?? string.Empty)
                ? _assignSkillButtonRect
                : _potentialButtonRects[actionIndex];
        }

        private static int GetIntrinsicListCount()
        {
            Char me = Char.myCharz();
            return me != null && me.infoSpeacialSkill != null && me.infoSpeacialSkill.Length > 0
                && me.infoSpeacialSkill[0] != null ? me.infoSpeacialSkill[0].Length : 0;
        }

        private static bool HasIntrinsicListData()
        {
            return GetIntrinsicListCount() > 0;
        }

        private void EnterIntrinsicListView()
        {
            _waitingIntrinsicList = false;
            _showIntrinsicList = true;
            _showIntrinsicConfirmation = false;
            _showSkillKeyPicker = false;
            _selectedIntrinsicListIndex = -1;
            _selectedIntrinsicSideAction = 0;
            _skillFocusArea = SkillFocusDetail;
            GameCanvas.panel?.hide();
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
        }

        private void ExitIntrinsicListView(bool playSound = true)
        {
            bool wasVisible = _showIntrinsicList || _showIntrinsicInput;
            _waitingIntrinsicList = false;
            _showIntrinsicList = false;
            CloseIntrinsicInput();
            _selectedIntrinsicListIndex = -1;
            _selectedIntrinsicSideAction = 0;
            if (_selectedSkillRow == IntrinsicRowIndex) _skillFocusArea = SkillFocusDetail;
            _rightScrollAdapter?.Reset();
            ConfigureScrollAdapters();
            if (playSound && wasVisible) SoundMn.gI().panelClick();
        }

        private string GetSelectedIntrinsicInfo()
        {
            Char me = Char.myCharz();
            if (me == null || me.infoSpeacialSkill == null || me.infoSpeacialSkill.Length == 0
                || me.infoSpeacialSkill[0] == null || _selectedIntrinsicListIndex < 0
                || _selectedIntrinsicListIndex >= me.infoSpeacialSkill[0].Length) return string.Empty;
            return me.infoSpeacialSkill[0][_selectedIntrinsicListIndex] ?? string.Empty;
        }

        private void OpenIntrinsicInput()
        {
            string selectedInfo = GetSelectedIntrinsicInfo();
            if (string.IsNullOrEmpty(selectedInfo)) return;
            _showIntrinsicInput = true;
            _intrinsicInputFocus = 0;
            _intrinsicInputField = new TField();
            _intrinsicInputField.name = "Nhập chỉ số mong muốn....";
            _intrinsicInputField.setIputType(TField.INPUT_TYPE_NUMERIC);
            _intrinsicInputField.setMaxTextLenght(3);
            ConfigureSkillActionRects();
            _intrinsicInputField.setFocusWithKb(true);
            GameCanvas.keyAsciiPress = 0;
            GameCanvas.clearKeyPressed();
            SoundMn.gI().panelClick();
        }

        private void CloseIntrinsicInput()
        {
            _showIntrinsicInput = false;
            if (_intrinsicInputField != null) _intrinsicInputField.setFocus(false);
            _intrinsicInputField = null;
            _intrinsicInputFocus = 0;
        }

        private void SetIntrinsicInputFocus(int focus)
        {
            _intrinsicInputFocus = focus;
            if (_intrinsicInputField != null) _intrinsicInputField.setFocus(focus == 0);
            SoundMn.gI().panelClick();
        }

        private void HandleIntrinsicInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                if (GameCanvas.isPointer(_intrinsicInputCloseRect.X, _intrinsicInputCloseRect.Y,
                    _intrinsicInputCloseRect.Width, _intrinsicInputCloseRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    CloseIntrinsicInput();
                    return;
                }
                if (_intrinsicInputField != null && GameCanvas.isPointer(_intrinsicInputField.x, _intrinsicInputField.y,
                    _intrinsicInputField.width, _intrinsicInputField.height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SetIntrinsicInputFocus(0);
                    _intrinsicInputField.setFocusWithKb(true);
                    return;
                }
                if (GameCanvas.isPointer(_intrinsicNormalButtonRect.X, _intrinsicNormalButtonRect.Y,
                    _intrinsicNormalButtonRect.Width, _intrinsicNormalButtonRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SetIntrinsicInputFocus(1);
                    SubmitIntrinsicInput(false);
                    return;
                }
                if (GameCanvas.isPointer(_intrinsicVipButtonRect.X, _intrinsicVipButtonRect.Y,
                    _intrinsicVipButtonRect.Width, _intrinsicVipButtonRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SetIntrinsicInputFocus(2);
                    SubmitIntrinsicInput(true);
                    return;
                }
                GameCanvas.clearAllPointerEvent();
            }

            if (GameCanvas.keyAsciiPress == 27 || GameCanvas.keyPressed[12] || GameCanvas.keyPressed[13])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                CloseIntrinsicInput();
                return;
            }

            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                SetIntrinsicInputFocus(0);
                return;
            }
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                SetIntrinsicInputFocus(_intrinsicInputFocus == 2 ? 2 : 1);
                return;
            }
            if (GameCanvas.keyPressed[23] || GameCanvas.keyPressed[4])
            {
                ConsumeDirectionKeys(23, 4);
                if (_intrinsicInputFocus > 0) SetIntrinsicInputFocus(1);
                return;
            }
            if (GameCanvas.keyPressed[24] || GameCanvas.keyPressed[6])
            {
                ConsumeDirectionKeys(24, 6);
                if (_intrinsicInputFocus > 0) SetIntrinsicInputFocus(2);
                return;
            }

            if (GameCanvas.keyPressed[Main.isPC ? 25 : 5])
            {
                GameCanvas.keyPressed[25] = false;
                GameCanvas.keyPressed[15] = false;
                GameCanvas.keyPressed[5] = false;
                if (_intrinsicInputFocus == 0) SetIntrinsicInputFocus(1);
                else SubmitIntrinsicInput(_intrinsicInputFocus == 2);
                return;
            }

            int ascii = GameCanvas.keyAsciiPress;
            if (_intrinsicInputFocus == 0 && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _intrinsicInputField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
            }
        }

        private void SubmitIntrinsicInput(bool vip)
        {
            string value = _intrinsicInputField != null ? _intrinsicInputField.getText() : string.Empty;
            if (!int.TryParse(value, out int target) || target <= 0)
            {
                GameScr.info1.addInfo("Chỉ số đã nhập không hợp lệ.", 0);
                return;
            }
            string selectedInfo = GetSelectedIntrinsicInfo();
            if (string.IsNullOrEmpty(selectedInfo)) return;
            ModFunc.GI().curSelectIntrinsic = selectedInfo;
            ModFunc.GI().SetAutoIntrinsic(target, vip);
            CloseIntrinsicInput();
            ExitIntrinsicListView(false);
        }

        private static long GetPotentialCurrentValue(Char me, int row)
        {
            if (me == null) return 0L;
            if (row == 0) return me.cHPGoc;
            if (row == 1) return me.cMPGoc;
            if (row == 2) return me.cDamGoc;
            if (row == 3) return me.cDefGoc;
            if (row == 4) return me.cCriticalGoc;
            return 0L;
        }

        private static long GetPotentialCost(Char me, int row)
        {
            if (me == null) return 0L;
            if (row == 0) return me.cHPGoc + 1000L;
            if (row == 1) return me.cMPGoc + 1000L;
            if (row == 2) return me.cDamGoc * (long)me.expForOneAdd;
            if (row == 3) return 500000L + me.cDefGoc * 100000L;
            if (row == 4 && Panel.t_tiemnang != null && Panel.t_tiemnang.Length > 0)
            {
                int level = System.Math.Max(0, System.Math.Min(me.cCriticalGoc, Panel.t_tiemnang.Length - 1));
                return Panel.t_tiemnang[level];
            }
            return 0L;
        }

        private static long GetPotentialBatchCost(Char me, int row, int batch)
        {
            if (me == null || batch <= 0) return 0L;
            if (row == 0) return batch * (2L * (me.cHPGoc + 1000L) + (batch - 1L) * 20L) / 2L;
            if (row == 1) return batch * (2L * (me.cMPGoc + 1000L) + (batch - 1L) * 20L) / 2L;
            if (row == 2) return batch * (2L * me.cDamGoc + batch - 1L) / 2L * me.expForOneAdd;
            if (row == 3) return batch * (2L * (me.cDefGoc + 5L) + batch - 1L) / 2L * 100000L;
            if (row == 4 && Panel.t_tiemnang != null && Panel.t_tiemnang.Length > 0)
            {
                long total = 0L;
                for (int i = 0; i < batch; i++)
                {
                    int level = System.Math.Max(0, System.Math.Min(me.cCriticalGoc + i, Panel.t_tiemnang.Length - 1));
                    total += Panel.t_tiemnang[level];
                }
                return total;
            }
            return 0L;
        }

        private static int GetPotentialBatch(int actionIndex)
        {
            return actionIndex == 0 ? 1 : (actionIndex == 1 ? 10 : 100);
        }

        private bool IsPotentialActionVisible(int actionIndex)
        {
            Char me = Char.myCharz();
            if (actionIndex < 0 || actionIndex >= _potentialButtonRects.Length || me == null
                || _selectedSkillRow < 0 || _selectedSkillRow >= PotentialStatRowCount) return false;
            // The original panel only supports a single critical upgrade per request.
            if (_selectedSkillRow == 4 && actionIndex != 0) return false;
            int batch = actionIndex == 3 ? 1 : GetPotentialBatch(actionIndex);
            long required = GetPotentialBatchCost(me, _selectedSkillRow, batch);
            return required > 0L && me.cTiemNang >= required;
        }

        private int GetFirstVisiblePotentialAction()
        {
            for (int i = 0; i < _potentialButtonRects.Length; i++)
                if (IsPotentialActionVisible(i)) return i;
            return -1;
        }

        private void IncreaseSelectedPotential(int batch)
        {
            Char me = Char.myCharz();
            if (me == null || _selectedSkillRow < 0 || _selectedSkillRow >= PotentialStatRowCount) return;
            if (me.statusMe == 14)
            {
                GameCanvas.startOKDlg(mResources.can_not_do_when_die);
                return;
            }

            long required = GetPotentialBatchCost(me, _selectedSkillRow, batch);
            if (required <= 0L || me.cTiemNang < required)
            {
                GameCanvas.startOKDlg("Không đủ tiềm năng. Cần " + NinjaUtil.getMoneys(required) + ".", isError: false);
                return;
            }

            Service.gI().upPotential(false, _selectedSkillRow, batch);
            SoundMn.gI().panelClick();
        }

        private void OpenAutoPotentialInput()
        {
            if (_selectedSkillRow < 0 || _selectedSkillRow >= PotentialStatRowCount || !IsPotentialActionVisible(3)) return;
            _showSkillKeyPicker = false;
            Close();
            ModFunc.GI().perform(100, _selectedSkillRow + "-False");
        }

        private SkillTemplate GetSelectedSkillTemplate()
        {
            int templateIndex = _selectedSkillRow - SkillTemplateStartRow;
            Char me = Char.myCharz();
            if (templateIndex < 0 || me == null || me.nClass == null || me.nClass.skillTemplates == null
                || templateIndex >= me.nClass.skillTemplates.Length) return null;
            return me.nClass.skillTemplates[templateIndex];
        }

        private Skill GetSelectedSkill()
        {
            SkillTemplate template = GetSelectedSkillTemplate();
            Char me = Char.myCharz();
            return template != null && me != null ? me.getSkill(template) : null;
        }

        private void OpenSkillKeyPicker()
        {
            Skill skill = GetSelectedSkill();
            if (skill == null)
            {
                GameCanvas.startOKDlg("Kỹ năng chưa được học.", isError: false);
                return;
            }
            bool useTouchSlots = GameCanvas.isTouch && !Main.isPC;
            Skill[] slots = useTouchSlots ? GameScr.onScreenSkill : GameScr.keySkill;
            _selectedSkillKeyIndex = 0;
            if (slots != null)
            {
                int limit = System.Math.Min(SkillKeyButtonCount, slots.Length);
                for (int i = 0; i < limit; i++)
                {
                    if (slots[i] == null || slots[i].template == null || slots[i].template.id != skill.template.id) continue;
                    _selectedSkillKeyIndex = i;
                    break;
                }
            }
            _showSkillKeyPicker = true;
            _skillFocusArea = SkillFocusKeys;
            SoundMn.gI().panelClick();
        }

        private void AssignSelectedSkillToKey(int keyIndex)
        {
            Skill skill = GetSelectedSkill();
            if (skill == null) return;
            bool useTouchSlots = GameCanvas.isTouch && !Main.isPC;
            Skill[] slots = useTouchSlots ? GameScr.onScreenSkill : GameScr.keySkill;
            if (slots == null || keyIndex < 0 || keyIndex >= slots.Length) return;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].template != null && slots[i].template.id == skill.template.id)
                    slots[i] = null;
            }
            slots[keyIndex] = skill;
            if (useTouchSlots) GameScr.gI().saveonScreenSkillToRMS();
            else GameScr.gI().saveKeySkillToRMS();
            _showSkillKeyPicker = false;
            _skillFocusArea = SkillFocusDetail;
            SoundMn.gI().panelClick();
        }

        private void PaintInventoryTabContent(mGraphics g)
        {
            PaintInventoryLeftTabs(g);
            PaintInventoryBagTabs(g);
            if (_selectedInventoryLeftTab == 0) PaintEquipmentSlots(g);
            else PaintPlayerInformation(g);
            if (ModFunc.isInventory) PaintInventoryGrid(g);
            else PaintInventoryList(g);
            PaintInventoryItemDetail(g);
        }

        private void PaintInventoryLeftTabs(mGraphics g)
        {
            PaintInventoryHeaderTab(g, _inventoryLeftTab0Rect, "Trang bị", _selectedInventoryLeftTab == 0);
            PaintInventoryHeaderTab(g, _inventoryLeftTab1Rect, "Thông tin", _selectedInventoryLeftTab == 1);
        }

        private void PaintInventoryBagTabs(mGraphics g)
        {
            PaintInventoryHeaderTab(g, _inventoryBagTab0Rect, "Tab 1", _selectedInventoryBagTab == 0);
            PaintInventoryHeaderTab(g, _inventoryBagTab1Rect, "Tab 2", _selectedInventoryBagTab == 1);
        }

        private static void PaintInventoryHeaderTab(mGraphics g, UiRect rect, string label, bool selected)
        {
            g.setColor(selected ? 0xD93A2E : 0xE9E1D5);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(selected ? 0xB52B23 : 0xB7A080);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);
            mFont font = selected ? mFont.tahoma_7b_white : mFont.tahoma_7_orange;
            font.drawString(g, label, rect.X + rect.Width / 2, rect.Y + 4, mFont.CENTER);
        }

        private void PaintEquipmentSlots(mGraphics g)
        {
            g.setColor(0xDFD2BC);
            g.fillRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);
            g.setColor(0xC4B79B);
            g.drawRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);

            Char me = Char.myCharz();
            Item[] body = me != null && me.arrItemBody != null ? me.arrItemBody : new Item[0];
            using (UiRenderState.Push(g, _leftColRect, clip: true))
            {
                if (me != null)
                {
                    int characterBottom = _equipmentSlotRects[6].Y - 3;
                    me.paintCharBody(g, _leftColRect.X + _leftColRect.Width / 2, characterBottom, 1, 0, true);
                }

                for (int visualIndex = 0; visualIndex < InventoryEquipmentSlotCount; visualIndex++)
                {
                    int slot = EquipmentVisualOrder[visualIndex];
                    Item item = slot < body.Length ? body[slot] : null;
                    UiRect rect = _equipmentSlotRects[visualIndex];
                    bool selected = slot == _selectedInventoryBodySlot;
                    PaintInventoryItemCell(g, item, rect, selected, 0xB7A489);

                    if (item == null || item.template == null)
                    {
                        PaintEquipmentSlotLabel(g, rect, EquipmentSlotLabels[slot]);
                    }
                }
            }
        }

        private static void PaintEquipmentSlotLabel(mGraphics g, UiRect rect, string label)
        {
            string[] lines = label.Split('\n');
            int lineHeight = 8;
            int y = rect.Y + (rect.Height - lines.Length * lineHeight) / 2;
            for (int i = 0; i < lines.Length; i++)
            {
                mFont.tahoma_7_white.drawString(g, lines[i], rect.X + rect.Width / 2, y + i * lineHeight, mFont.CENTER);
            }
        }

        private void PaintPlayerInformation(mGraphics g)
        {
            g.setColor(0xDFD2BC);
            g.fillRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);
            g.setColor(0xC4B79B);
            g.drawRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);

            Char me = Char.myCharz();
            if (me == null) return;
            using (UiRenderState.Push(g, _leftColRect, clip: true))
            {
                int scrollY = _leftScrollAdapter != null ? _leftScrollAdapter.ScrollY : 0;
                int x = _leftColRect.X + 5;
                int y = _leftColRect.Y + 4 - scrollY;
                int avatarWidth = System.Math.Min(62, System.Math.Max(48, _leftColRect.Width / 3));
                UiRect profileAvatarRect = new UiRect(x, y + 2, avatarWidth, PlayerProfileHeaderHeight - 8);
                using (UiRenderState.Push(g, profileAvatarRect, clip: true))
                {
                    SmallImage.drawSmallImage(g, me.avatarz(), profileAvatarRect.X + profileAvatarRect.Width / 2,
                        profileAvatarRect.Bottom - 2, 0, mGraphics.HCENTER | mGraphics.BOTTOM);
                }

                int textX = profileAvatarRect.Right + 6;
                int textWidth = System.Math.Max(45, _leftColRect.Right - textX - 5);
                mFont.tahoma_7_orange.drawString(g, TruncateString(mFont.tahoma_7_orange, me.cName, textWidth), textX, y + 7, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, "Tộc : " + GetRaceName(me.cgender), textX, y + 25, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, TruncateString(mFont.tahoma_7b_dark, me.getStrLevel(), textWidth), textX, y + 42, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, "Sức mạnh :", textX, y + 59, mFont.LEFT);
                mFont.tahoma_7b_dark.drawString(g, NinjaUtil.getMoneys(me.cPower), textX, y + 74, mFont.LEFT);

                int dividerY = y + PlayerProfileHeaderHeight - 4;
                g.setColor(0xB58C58);
                g.drawLine(_leftColRect.X + 3, dividerY, _leftColRect.X + _leftColRect.Width - 3, dividerY);
                mFont.tahoma_7_red.drawString(g, "Thông tin player", x, dividerY + 4, mFont.LEFT);
                int lineY = dividerY + 19;
                int lineWidth = _leftColRect.Width - 10;
                List<string> infoLines = BuildPlayerInformationLines(me);
                for (int i = 0; i < infoLines.Count; i++)
                {
                    string line = infoLines[i];
                    if (string.IsNullOrEmpty(line))
                    {
                        lineY += PlayerInfoLineHeight;
                        continue;
                    }
                    mFont font = line == "Chỉ số từ trang bị:" ? mFont.tahoma_7_red : mFont.tahoma_7b_dark;
                    string[] wrapped = font.splitFontArray(line, lineWidth);
                    for (int j = 0; j < wrapped.Length; j++)
                    {
                        font.drawString(g, wrapped[j], x, lineY, mFont.LEFT);
                        lineY += PlayerInfoLineHeight;
                    }
                }
            }
        }

        private static List<string> BuildPlayerInformationLines(Char character)
        {
            List<string> lines = new List<string>();
            if (character == null) return lines;
            lines.Add("Tộc: " + GetRaceName(character.cgender));
            lines.Add("HP: " + NinjaUtil.getMoneys(character.cHP) + " / " + NinjaUtil.getMoneys(character.cHPFull));
            lines.Add("KI: " + NinjaUtil.getMoneys(character.cMP) + " / " + NinjaUtil.getMoneys(character.cMPFull));
            lines.Add("Sức đánh: " + NinjaUtil.getMoneys(character.cDamFull));
            lines.Add("Giáp: " + NinjaUtil.getMoneys(character.cDefull));
            lines.Add("Chí mạng: " + character.cCriticalFull + "% / SĐCM: " + (100 + SumEquippedOption(character, 5)) + "%");
            if (character.tlDef > 0) lines.Add("Giảm sát thương: " + character.tlDef + "%");
            if (character.tlPst > 0) lines.Add("Phản sát thương: " + character.tlPst + "%");
            if (character.tlNeDon > 0) lines.Add("Né đòn: " + character.tlNeDon + "%");
            if (character.tlHutHp > 0) lines.Add("Hút HP: " + character.tlHutHp + "%");
            if (character.tlHutMp > 0) lines.Add("Hút KI: " + character.tlHutMp + "%");
            if (character.tileGiamTDHS > 0) lines.Add("Giảm TDHS: " + character.tileGiamTDHS + "%");
            if (character.timeGiamTDHS > 0) lines.Add("Giảm TDHS: " + character.timeGiamTDHS + " giây");
            if (character.khangTDHS) lines.Add("Kháng TDHS: Có");
            if (character.isKhongLanh) lines.Add("Kháng lạnh: Có");
            if (character.wearingVoHinh) lines.Add("Vô hình: Có");
            if (character.teleport) lines.Add("Dịch chuyển: Có");

            Dictionary<int, int> optionTotals = new Dictionary<int, int>();
            Dictionary<int, ItemOptionTemplate> optionTemplates = new Dictionary<int, ItemOptionTemplate>();
            List<int> optionOrder = new List<int>();
            if (character.arrItemBody != null)
            {
                for (int i = 0; i < character.arrItemBody.Length; i++)
                {
                    Item item = character.arrItemBody[i];
                    if (item == null || item.itemOption == null) continue;
                    for (int j = 0; j < item.itemOption.Length; j++)
                    {
                        ItemOption option = item.itemOption[j];
                        if (option == null || option.optionTemplate == null || !option.IsValidOption()
                            || string.IsNullOrEmpty(option.optionTemplate.name)) continue;
                        int id = option.optionTemplate.id;
                        if (!optionTotals.ContainsKey(id))
                        {
                            optionTotals.Add(id, 0);
                            optionTemplates.Add(id, option.optionTemplate);
                            optionOrder.Add(id);
                        }
                        optionTotals[id] += option.param;
                    }
                }
            }

            lines.Add(string.Empty);
            lines.Add("Chỉ số từ trang bị:");
            if (optionOrder.Count == 0)
            {
                lines.Add("Chưa trang bị vật phẩm có chỉ số.");
            }
            else
            {
                for (int i = 0; i < optionOrder.Count; i++)
                {
                    int id = optionOrder[i];
                    ItemOptionTemplate template = optionTemplates[id];
                    string optionText = template.name.StartsWith("$")
                        ? NinjaUtil.Replace(template.name, "$", string.Empty)
                        : NinjaUtil.Replace(template.name, "#", optionTotals[id] + string.Empty);
                    lines.Add("- " + optionText);
                }
            }
            return lines;
        }

        private static string GetRaceName(int gender)
        {
            if (gender == 1) return "Namek";
            if (gender == 2) return "Xayda";
            return "Trái Đất";
        }

        private static int SumEquippedOption(Char character, int optionId)
        {
            int total = 0;
            if (character == null || character.arrItemBody == null) return total;
            for (int i = 0; i < character.arrItemBody.Length; i++)
            {
                Item item = character.arrItemBody[i];
                if (item == null || item.itemOption == null) continue;
                for (int j = 0; j < item.itemOption.Length; j++)
                {
                    ItemOption option = item.itemOption[j];
                    if (option != null && option.optionTemplate != null && option.optionTemplate.id == optionId) total += option.param;
                }
            }
            return total;
        }

        private void PaintInventoryGrid(mGraphics g)
        {
            PaintInventoryBagBackground(g);
            Char me = Char.myCharz();
            Item[] bag = me != null && me.arrItemBag != null ? me.arrItemBag : new Item[0];
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bag.Length);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bag.Length);
            const int gap = 2;
            int cellWidth = System.Math.Max(1, (_rightBodyRect.Width - gap * (InventoryGridColumns + 1)) / InventoryGridColumns);
            int cellHeight = InventoryGridRowHeight - gap;
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    int row = localIndex / InventoryGridColumns;
                    int column = localIndex % InventoryGridColumns;
                    int x = _rightBodyRect.X + gap + column * (cellWidth + gap);
                    int y = _rightBodyRect.Y + gap + row * InventoryGridRowHeight - scrollY;
                    if (y + cellHeight <= _rightBodyRect.Y || y >= _rightBodyRect.Bottom) continue;
                    int bagIndex = start + localIndex;
                    Item item = bagIndex < bag.Length ? bag[bagIndex] : null;
                    bool selected = bagIndex == _selectedInventoryBagSlot;
                    UiRect rect = new UiRect(x, y, cellWidth, cellHeight);
                    PaintInventoryItemCell(g, item, rect, selected, 0xB7A489);
                }
            }
        }

        private void PaintInventoryList(mGraphics g)
        {
            PaintInventoryBagBackground(g);
            Char me = Char.myCharz();
            Item[] bag = me != null && me.arrItemBag != null ? me.arrItemBag : new Item[0];
            int start = GetInventoryTabStart(_selectedInventoryBagTab, bag.Length);
            int count = GetInventoryTabItemCount(_selectedInventoryBagTab, bag.Length);
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            const int iconWidth = 36;

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                for (int localIndex = 0; localIndex < count; localIndex++)
                {
                    int y = _rightBodyRect.Y + localIndex * InventoryListRowHeight - scrollY;
                    if (y + InventoryListRowHeight <= _rightBodyRect.Y || y >= _rightBodyRect.Bottom) continue;
                    int bagIndex = start + localIndex;
                    Item item = bagIndex < bag.Length ? bag[bagIndex] : null;
                    bool selected = bagIndex == _selectedInventoryBagSlot;
                    g.setColor(selected ? 0xFFD038 : 0xF2F0ED);
                    g.fillRect(_rightBodyRect.X + iconWidth + 2, y, _rightBodyRect.Width - iconWidth - 2, InventoryListRowHeight - 1);
                    g.setColor(selected ? 0xD7A900 : 0xB7A489);
                    g.fillRect(_rightBodyRect.X, y, iconWidth, InventoryListRowHeight - 1);
                    g.setColor(0xD7C8B2);
                    g.drawRect(_rightBodyRect.X, y, _rightBodyRect.Width, InventoryListRowHeight - 1);
                    if (item == null || item.template == null) continue;

                    UiRect iconRect = new UiRect(_rightBodyRect.X, y, iconWidth, InventoryListRowHeight - 1);
                    PaintInventoryItemCell(g, item, iconRect, selected, 0xB7A489);

                    int textX = _rightBodyRect.X + iconWidth + 7;
                    int textWidth = _rightBodyRect.Width - iconWidth - 11;
                    string name = item.template.name + GetUpgradeSuffix(item);
                    mFont.tahoma_7b_dark.drawString(g, TruncateString(mFont.tahoma_7b_dark, name, textWidth), textX, y + 5, mFont.LEFT);
                    string summary = GetItemOptionSummary(item);
                    if (!string.IsNullOrEmpty(summary))
                        mFont.tahoma_7_blue.drawString(g, TruncateString(mFont.tahoma_7_blue, summary, textWidth), textX, y + 20, mFont.LEFT);
                    if (selected)
                        PaintInventoryFocusFrame(g, new UiRect(_rightBodyRect.X, y,
                            _rightBodyRect.Width, InventoryListRowHeight - 1));
                }
            }
        }

        private void PaintInventoryBagBackground(mGraphics g)
        {
            g.setColor(0xDFD2BC);
            g.fillRect(_rightBodyRect.X, _rightBodyRect.Y, _rightBodyRect.Width, _rightBodyRect.Height);
            g.setColor(0xC4B79B);
            g.drawRect(_rightBodyRect.X, _rightBodyRect.Y, _rightBodyRect.Width, _rightBodyRect.Height);
        }

        private static void PaintInventoryItemCell(mGraphics g, Item item, UiRect rect, bool focused, int defaultColor)
        {
            g.setColor(defaultColor);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            if (GameCanvas.panel != null) GameCanvas.panel.paintInventoryGridEffect(g, item,
                rect.X, rect.Y, rect.Width, rect.Height, includeUpgradeLevel: false, includeCrystalStars: false);
            g.setColor(0xDFD2BC, 0.85f);
            g.drawRect(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            if (GetItemUpgradeLevel(item) > 0)
                Panel.paintEquipmentCellFrame(g, item, rect.X, rect.Y, rect.Width, rect.Height,
                    isSelected: false, inset: InventoryEquipmentBorderInset);
            if (item != null && item.template != null)
            {
                SmallImage.drawSmallImage(g, item.template.iconID, rect.X + rect.Width / 2,
                    rect.Y + rect.Height / 2, 0, 3);
                if (GameCanvas.panel != null) GameCanvas.panel.paintInventoryGridItemMarkers(g, item,
                    rect.X, rect.Y, rect.Width, rect.Height, paintCrystalSlotBorder: false);
                PaintInventoryItemMarkers(g, item, rect);
            }
            if (focused) PaintInventoryFocusFrame(g, rect);
        }

        private static void PaintInventoryFocusFrame(mGraphics g, UiRect rect)
        {
            float glow = GameCanvas.gameTick % 20 < 10 ? 0.95f : 0.65f;
            g.setColor(0xFFF1A0, glow);
            g.drawRect(rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            g.setColor(0xF3A000, 0.9f);
            g.drawRect(rect.X + 1, rect.Y + 1, System.Math.Max(1, rect.Width - 3),
                System.Math.Max(1, rect.Height - 3));
        }

        private static void PaintInventoryItemMarkers(mGraphics g, Item item, UiRect rect)
        {
            int upgrade = GetItemUpgradeLevel(item);
            if (upgrade > 0)
                mFont.tahoma_7_yellow.drawString(g, "+" + upgrade, rect.X + 2, rect.Y + 1, mFont.LEFT);
            if (item.quantity > 1)
                mFont.tahoma_7_yellow.drawString(g, item.quantity.ToString(), rect.Right - 2,
                    rect.Bottom - mFont.tahoma_7_yellow.getHeight(), mFont.RIGHT);
        }

        private void PaintInventoryItemDetail(mGraphics g)
        {
            if (!_showInventoryDetail) return;
            Item item = GetSelectedInventoryItem(out bool fromBody, out _);
            if (item == null || item.template == null) return;
            ConfigureInventoryDetailRects();
            UiRect rect = _inventoryDetailRect;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            g.setColor(0x35281C, 0.35f);
            g.fillRect(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
            g.setColor(0xF5F1EA);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(0xC9B89F);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);

            using (UiRenderState.Push(g, rect, clip: true))
            {
                const int headerHeight = 39;
                g.setColor(0xE8DDCC);
                g.fillRect(rect.X + 1, rect.Y + 1, rect.Width - 2, headerHeight - 1);
                int iconWidth = System.Math.Min(34, rect.Width / 4);
                UiRect iconRect = new UiRect(rect.X + 4, rect.Y + 3, iconWidth, 32);
                PaintInventoryItemCell(g, item, iconRect, false, 0xB7A489);
                int textX = iconRect.Right + 6;
                int textWidth = System.Math.Max(30, rect.Right - textX - 4);
                int y = rect.Y + 5;
                mFont.tahoma_7b_dark.drawString(g,
                    TruncateString(mFont.tahoma_7b_dark, item.template.name + GetUpgradeSuffix(item), textWidth),
                    textX, y, mFont.LEFT);
                y += 15;
                if (item.quantity > 1)
                {
                    mFont.tahoma_7_green2.drawString(g, "Số lượng: " + item.quantity, textX, y, mFont.LEFT);
                }

                int headerDividerY = rect.Y + headerHeight;
                g.setColor(0xC9B89F);
                g.drawLine(rect.X + 4, headerDividerY, rect.Right - 4, headerDividerY);
                GetCrystalStarSlots(item, out _, out int maxStarSlots);
                int starHeight = maxStarSlots > 0 ? (maxStarSlots > 5 ? 29 : 18) : 0;
                bool hasFooter = item.template.strRequire > 1 || !string.IsNullOrEmpty(item.template.description);
                int footerHeight = hasFooter ? 20 : 0;
                int optionsBottom = rect.Bottom - 3 - starHeight - footerHeight;
                y = headerDividerY + 4;
                int optionTextX = rect.X + 7;
                int optionTextWidth = System.Math.Max(30, rect.Width - 14);

                if (item.itemOption != null)
                {
                    for (int i = 0; i < item.itemOption.Length && y < optionsBottom; i++)
                    {
                        ItemOption option = item.itemOption[i];
                        if (option == null || option.optionTemplate == null || !option.IsValidOption()) continue;
                        int id = option.optionTemplate.id;
                        if (id == Item.OPT_LVITEM || id == Item.OPT_STARSLOT || id == Item.OPT_MAXSTARSLOT) continue;
                        string optionText = option.getOptionString();
                        if (string.IsNullOrEmpty(optionText)) continue;
                        string[] wrapped = mFont.tahoma_7_green2.splitFontArray(optionText, optionTextWidth);
                        for (int j = 0; j < wrapped.Length && y < optionsBottom; j++)
                        {
                            mFont.tahoma_7_green2.drawString(g, wrapped[j], optionTextX, y, mFont.LEFT);
                            y += 13;
                        }
                    }
                }

                int footerY = rect.Bottom - starHeight - footerHeight;
                if (hasFooter)
                {
                    g.setColor(0xB58C58);
                    g.drawLine(rect.X + 5, footerY, rect.Right - 5, footerY);
                    if (item.template.strRequire > 1)
                    {
                        mFont requirementFont = item.template.strRequire > Char.myCharz().cPower
                            ? mFont.tahoma_7_red : mFont.tahoma_7_grey;
                        requirementFont.drawString(g, "Sức mạnh yêu cầu: " + item.template.strRequire,
                            rect.X + rect.Width / 2, footerY + 4, mFont.CENTER);
                    }
                    else
                    {
                        mFont.tahoma_7_grey.drawString(g,
                            TruncateString(mFont.tahoma_7_grey, item.template.description, rect.Width - 10),
                            rect.X + rect.Width / 2, footerY + 4, mFont.CENTER);
                    }
                }
                if (starHeight > 0)
                {
                    int starY = rect.Bottom - starHeight;
                    g.setColor(0xB58C58);
                    g.drawLine(rect.X + 5, starY, rect.Right - 5, starY);
                    PaintCrystalStars(g, item, new UiRect(rect.X + 5, starY + 2, rect.Width - 10, starHeight - 2));
                }
            }

            int actionCount = GetInventoryActionCount(item, fromBody);
            for (int i = 0; i < actionCount; i++)
            {
                UiRect actionRect = _inventoryActionRects[i];
                bool focused = _inventoryFocusArea == InventoryFocusActions && _selectedInventoryAction == i;
                PaintInventoryActionButton(g, actionRect, GetInventoryActionLabel(item, fromBody, i), focused);
            }
        }

        private static void GetCrystalStarSlots(Item item, out int filledSlots, out int maxSlots)
        {
            filledSlots = 0;
            maxSlots = 0;
            if (item == null || item.itemOption == null) return;
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                ItemOption option = item.itemOption[i];
                if (option == null || option.optionTemplate == null) continue;
                if (option.optionTemplate.id == Item.OPT_STARSLOT) filledSlots = System.Math.Max(0, option.param);
                else if (option.optionTemplate.id == Item.OPT_MAXSTARSLOT) maxSlots = System.Math.Max(0, option.param);
            }
            if (filledSlots > maxSlots) filledSlots = maxSlots;
        }

        private static void PaintCrystalStars(mGraphics g, Item item, UiRect rect)
        {
            GetCrystalStarSlots(item, out int filledSlots, out int maxSlots);
            if (maxSlots <= 0) return;
            int topCount = maxSlots > 5 ? (maxSlots + 1) / 2 : maxSlots;
            int bottomCount = maxSlots - topCount;
            int spacing = System.Math.Min(20, System.Math.Max(9, (rect.Width - 4) / System.Math.Max(1, topCount)));
            for (int i = 0; i < maxSlots; i++)
            {
                bool bottomRow = i >= topCount;
                int rowIndex = bottomRow ? i - topCount : i;
                int rowCount = bottomRow ? bottomCount : topCount;
                int x = rect.X + rect.Width / 2 - (rowCount - 1) * spacing / 2 + rowIndex * spacing;
                int y = rect.Y + (bottomRow ? 18 : 7);
                Image starImage = i < filledSlots
                    ? (i >= ChatPopup.numSlot && Panel.imgStar8 != null ? Panel.imgStar8 : Panel.imgStar)
                    : Panel.imgMaxStar;
                if (starImage != null) g.drawImage(starImage, x, y, mGraphics.HCENTER | mGraphics.VCENTER);
                else
                {
                    mFont starFont = i < filledSlots ? mFont.tahoma_7b_yellow : mFont.tahoma_7b_dark;
                    starFont.drawString(g, "*", x, y - 5, mFont.CENTER);
                }
            }
        }

        private static void PaintInventoryActionButton(mGraphics g, UiRect rect, string label, bool focused)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            g.setColor(focused ? 0xFFD038 : 0xE99A00);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(focused ? 0xE17B00 : 0xF6C13A);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);
            string[] lines = label.Split('\n');
            int lineHeight = 10;
            int y = rect.Y + (rect.Height - lines.Length * lineHeight) / 2;
            for (int i = 0; i < lines.Length; i++)
                mFont.tahoma_7b_dark.drawString(g, lines[i], rect.X + rect.Width / 2, y + i * lineHeight, mFont.CENTER);
        }

        private static int GetItemUpgradeLevel(Item item)
        {
            if (item == null || item.itemOption == null) return 0;
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                ItemOption option = item.itemOption[i];
                if (option != null && option.optionTemplate != null && option.optionTemplate.id == 72) return option.param;
            }
            return 0;
        }

        private static string GetUpgradeSuffix(Item item)
        {
            int upgrade = GetItemUpgradeLevel(item);
            return upgrade > 0 ? " [+" + upgrade + "]" : string.Empty;
        }

        private static string GetItemOptionSummary(Item item)
        {
            if (item == null || item.itemOption == null) return string.Empty;
            for (int i = 0; i < item.itemOption.Length; i++)
            {
                ItemOption option = item.itemOption[i];
                if (option == null || option.optionTemplate == null) continue;
                int id = option.optionTemplate.id;
                if (id == 72 || id == 102 || id == 107) continue;
                return option.getOptionString();
            }
            return string.Empty;
        }

        private void EnsureClanChatField()
        {
            if (_clanChatField != null) return;
            _clanChatField = new TField();
            _clanChatField.name = "Soạn tin nhắn ...";
            _clanChatField.setIputType(TField.INPUT_TYPE_ANY);
            _clanChatField.setMaxTextLenght(120);
        }

        private void ConfigureClanRects()
        {
            _clanChatHeaderRect = _skillListHeaderRect;
            int composerHeight = 32;
            _clanChatComposerRect = new UiRect(_leftColRect.X, _leftColRect.Bottom - composerHeight,
                _leftColRect.Width, composerHeight);
            _clanChatListRect = new UiRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width,
                System.Math.Max(1, _leftColRect.Height - composerHeight - 2));
            _clanShareRect = new UiRect(_clanChatComposerRect.X + 2, _clanChatComposerRect.Y + 3, 31, 25);
            _clanSendRect = new UiRect(_clanChatComposerRect.Right - 39, _clanChatComposerRect.Y + 3, 37, 25);
            EnsureClanChatField();
            _clanChatField.x = _clanShareRect.Right + 3;
            _clanChatField.y = _clanChatComposerRect.Y + 3;
            _clanChatField.width = System.Math.Max(36, _clanSendRect.X - _clanChatField.x - 3);
            _clanChatField.height = 25;

            const int functionGap = 3;
            const int functionHeight = 22;
            int functionWidth = (_rightColRect.Width - functionGap) / 2;
            for (int i = 0; i < _clanFunctionRects.Length; i++)
            {
                int column = i % 2;
                int row = i / 2;
                _clanFunctionRects[i] = new UiRect(_rightColRect.X + column * (functionWidth + functionGap),
                    _rightColRect.Y + row * (functionHeight + functionGap), functionWidth, functionHeight);
            }
            _clanRightHeaderRect = new UiRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, 22);
            int bodyY = _selectedClanView == ClanViewHistory
                ? _clanRightHeaderRect.Bottom + 4
                : _rightColRect.Y + 3 * (functionHeight + functionGap);
            _clanRightBodyRect = new UiRect(_rightColRect.X, bodyY, _rightColRect.Width,
                System.Math.Max(1, _rightColRect.Bottom - bodyY));

            const int sideWidth = 54;
            const int sideHeight = 23;
            const int sideGap = 4;
            int sideX = _frameRect.Right + 3;
            if (sideX + sideWidth > GameCanvas.w - 2) sideX = System.Math.Max(2, GameCanvas.w - sideWidth - 2);
            for (int i = 0; i < _clanSideActionRects.Length; i++)
                _clanSideActionRects[i] = new UiRect(sideX, _frameRect.Y + 4 + i * (sideHeight + sideGap),
                    sideWidth, sideHeight);

            _clanUpgradeButtonRect = new UiRect(_clanRightBodyRect.X + (_clanRightBodyRect.Width - 100) / 2,
                _clanRightBodyRect.Bottom - 43, 100, 38);

            int dialogWidth = System.Math.Min(340, GameCanvas.w - 20);
            int dialogHeight = 122;
            int dialogX = (GameCanvas.w - dialogWidth) / 2;
            int dialogY = (GameCanvas.h - dialogHeight) / 2;
            _clanDialogRect = new UiRect(dialogX, dialogY, dialogWidth, dialogHeight);
            _clanDialogCloseRect = new UiRect(dialogX + dialogWidth - 23, dialogY + 5, 18, 18);
            _clanDialogSubmitRect = new UiRect(dialogX + (dialogWidth - 94) / 2,
                dialogY + dialogHeight - 35, 94, 27);
            if (_clanDialogField != null)
            {
                _clanDialogField.x = dialogX + 25;
                _clanDialogField.y = dialogY + 50;
                _clanDialogField.width = dialogWidth - 50;
                _clanDialogField.height = 28;
            }
        }

        private void EnterClanTab()
        {
            _clanFocusArea = ClanFocusFunctions;
            _selectedClanFunction = GetClanFunctionForView(_selectedClanView);
            _selectedClanRow = 0;
            _clanChatFocused = false;
            EnsureClanChatField();
            ConfigureClanRects();
            RefreshClanData();
        }

        private void RefreshClanData()
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            ClanProgression.requestSnapshot(false);
            ClanProgression.requestBuffSnapshot(false);
            Service.gI().clanTreasuryView();
            ClanValue.requestSnapshot(false);
            ClanAppearance.requestSnapshot(false);
        }

        private static MyVector GetClanMembers()
        {
            if (GameCanvas.panel != null && GameCanvas.panel.myMember != null)
                return GameCanvas.panel.myMember;
            return new MyVector();
        }

        private static Member FindClanMember(int playerId)
        {
            MyVector members = GetClanMembers();
            for (int i = 0; i < members.size(); i++)
            {
                Member member = members.elementAt(i) as Member;
                if (member != null && member.ID == playerId) return member;
            }
            return null;
        }

        private int GetClanContentItemCount()
        {
            if (_selectedClanView == ClanViewMembers) return GetClanMembers().size();
            if (_selectedClanView == ClanViewInfo) return BuildClanInfoLines().Count;
            if (_selectedClanView == ClanViewPotential) return ClanProgression.BRANCH_COUNT;
            if (_selectedClanView == ClanViewHistory) return GetClanContributionLedgerCount();
            if (_selectedClanView == ClanViewTreasury)
            {
                Char me = Char.myCharz();
                int count = me != null && me.arrItemBox != null ? me.arrItemBox.Length : 0;
                return (count + ClanStorageColumns - 1) / ClanStorageColumns;
            }
            return 0;
        }

        private int GetClanContentRowHeight()
        {
            if (_selectedClanView == ClanViewMembers) return ClanMemberRowHeight;
            if (_selectedClanView == ClanViewInfo) return 13;
            if (_selectedClanView == ClanViewPotential) return ClanPotentialRowHeight;
            if (_selectedClanView == ClanViewHistory) return ClanLedgerRowHeight;
            if (_selectedClanView == ClanViewTreasury) return ClanStorageRowHeight;
            return 10;
        }

        private UiRect GetClanScrollableBodyRect()
        {
            if (_selectedClanView == ClanViewInfo)
                return new UiRect(_clanRightBodyRect.X + 3, _clanRightBodyRect.Y + 3,
                    System.Math.Max(1, _clanRightBodyRect.Width - 6),
                    System.Math.Max(1, _clanRightBodyRect.Height - 7));
            if (_selectedClanView != ClanViewPotential) return _clanRightBodyRect;
            const int pointsHeaderHeight = 17;
            return new UiRect(_clanRightBodyRect.X, _clanRightBodyRect.Y + pointsHeaderHeight,
                _clanRightBodyRect.Width,
                System.Math.Max(1, _clanRightBodyRect.Height - pointsHeaderHeight));
        }

        private static int GetClanContributionLedgerCount()
        {
            int count = 0;
            for (int i = 0; i < ClanTreasury.current.ledger.size(); i++)
            {
                ClanLedgerEntry entry = ClanTreasury.current.ledger.elementAt(i) as ClanLedgerEntry;
                if (ClanTreasury.isPlayerContributionEntry(entry)) count++;
            }
            return count;
        }

        private static int GetClanFunctionForView(int view)
        {
            if (view == ClanViewInfo) return 1;
            if (view == ClanViewTreasury || view == ClanViewHistory) return 2;
            if (view == ClanViewPotential) return 3;
            if (view == ClanViewUpgrade) return 5;
            return 0;
        }

        private void SelectClanView(int view)
        {
            _selectedClanView = view;
            _selectedClanFunction = GetClanFunctionForView(view);
            _selectedClanRow = 0;
            _selectedClanStorageSlot = -1;
            ConfigureClanRects();
            _rightScrollAdapter?.Reset();
            if (view == ClanViewInfo) RefreshClanData();
            else if (view == ClanViewPotential || view == ClanViewUpgrade)
                ClanProgression.requestSnapshot(true);
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        private void OpenClanTreasury()
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            _selectedClanView = ClanViewTreasury;
            _selectedClanFunction = 2;
            _selectedClanRow = 0;
            _selectedClanStorageSlot = -1;
            _clanStorageLoaded = false;
            ConfigureClanRects();
            _rightScrollAdapter?.Reset();
            Service.gI().clanTreasuryView();
            Service.gI().clanItemStorageView();
            ConfigureScrollAdapters();
            SoundMn.gI().panelClick();
        }

        public static bool TryConsumeClanStorageOpen()
        {
            if (!_isOpen || _instance == null || _instance._selectedMainTab != 3
                || _instance._selectedClanView != ClanViewTreasury)
                return false;
            _instance._clanStorageLoaded = true;
            _instance._selectedClanStorageSlot = -1;
            _instance._rightScrollAdapter?.Reset();
            _instance.ConfigureScrollAdapters();
            ClanProgression.requestBuffSnapshot(true);
            return true;
        }

        private void PaintClanTabContent(mGraphics g)
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null)
            {
                g.setColor(0xDFD2BC);
                g.fillRect(_contentRect.X + 4, _contentRect.Y + 4, _contentRect.Width - 8, _contentRect.Height - 8);
                mFont.tahoma_7b_dark.drawString(g, "Bạn chưa tham gia bang hội.",
                    _contentRect.X + _contentRect.Width / 2, _contentRect.Y + _contentRect.Height / 2 - 8,
                    mFont.CENTER);
                mFont.tahoma_7_grey.drawString(g, "Hãy dùng menu Bang hội chính để tìm hoặc tạo bang.",
                    _contentRect.X + _contentRect.Width / 2, _contentRect.Y + _contentRect.Height / 2 + 10,
                    mFont.CENTER);
                return;
            }

            PaintClanChatColumn(g);
            if (_selectedClanView == ClanViewHistory)
            {
                PaintClanHeader(g, _clanRightHeaderRect, "Lịch sử cống hiến", true);
                PaintClanHistory(g);
            }
            else
            {
                PaintClanFunctionTabs(g);
                if (_selectedClanView == ClanViewMembers) PaintClanMembers(g);
                else if (_selectedClanView == ClanViewInfo) PaintClanInfo(g);
                else if (_selectedClanView == ClanViewTreasury) PaintClanTreasury(g);
                else if (_selectedClanView == ClanViewPotential) PaintClanPotential(g);
                else if (_selectedClanView == ClanViewUpgrade) PaintClanUpgrade(g);
            }
            PaintClanScrollbar(g, _leftScrollAdapter, _clanChatListRect);
            PaintClanScrollbar(g, _rightScrollAdapter, GetClanScrollableBodyRect());
            PaintClanSideActions(g);
        }

        private void PaintClanChatColumn(mGraphics g)
        {
            PaintClanHeader(g, _clanChatHeaderRect, "Chat bang", true);
            PaintClanSurface(g, _clanChatListRect, 0xDED1BB);
            PaintClanChatMessages(g);

            PaintClanSurface(g, _clanChatComposerRect, 0xEDE5D9);
            PaintClanButton(g, _clanShareRect, "Vị trí", false, _clanFocusArea == ClanFocusChat && !_clanChatFocused);
            _clanChatField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintClanButton(g, _clanSendRect, "Gửi", false, _clanFocusArea == ClanFocusChat && _clanChatFocused);
        }

        private void PaintClanChatMessages(mGraphics g)
        {
            int scrollY = _leftScrollAdapter != null ? _leftScrollAdapter.ScrollY : 0;
            using (UiRenderState.Push(g, _clanChatListRect, clip: true))
            {
                for (int i = 0; i < ClanMessage.vMessage.size(); i++)
                {
                    ClanMessage message = ClanMessage.vMessage.elementAt(i) as ClanMessage;
                    if (message == null) continue;
                    int y = _clanChatListRect.Y + i * ClanChatRowHeight - scrollY;
                    if (y + ClanChatRowHeight <= _clanChatListRect.Y || y >= _clanChatListRect.Bottom) continue;
                    UiRect rowRect = new UiRect(_clanChatListRect.X + 2, y + 1,
                        _clanChatListRect.Width - 7, ClanChatRowHeight - 3);
                    g.setColor(0xB9AA93);
                    g.fillRect(rowRect.X + 1, rowRect.Y + 1, rowRect.Width, rowRect.Height, 4);
                    g.setColor(0xF8F6F2);
                    g.fillRect(rowRect.X, rowRect.Y, rowRect.Width, System.Math.Max(1, rowRect.Height - 1), 4);
                    g.setColor(0xFFFFFF);
                    g.fillRect(rowRect.X + 3, rowRect.Y + 1, System.Math.Max(1, rowRect.Width - 6), 1);

                    UiRect avatarRect = new UiRect(rowRect.X + 3, rowRect.Y + 3, 37, rowRect.Height - 6);
                    g.setColor(0xB9A68A);
                    g.fillRect(avatarRect.X, avatarRect.Y, avatarRect.Width, avatarRect.Height, 4);
                    PaintClanMemberAvatar(g, FindClanMember(message.playerId), avatarRect);

                    int textX = avatarRect.Right + 5;
                    int visibleOptionCount = GetVisibleClanMessageOptionCount(message);
                    int rightReserve = System.Math.Min(80, visibleOptionCount * 38);
                    int textWidth = System.Math.Max(35, rowRect.Right - textX - 4 - rightReserve);
                    mFont nameFont = message.role == 0 ? mFont.tahoma_7b_red
                        : message.role == 1 ? mFont.tahoma_7b_green : mFont.tahoma_7b_blue;
                    nameFont.drawString(g, TruncateString(nameFont, message.playerName ?? string.Empty, textWidth),
                        textX, rowRect.Y + 3, mFont.LEFT);
                    string body = GetClanMessageText(message);
                    string[] lines = mFont.tahoma_7b_dark.splitFontArray(body, textWidth);
                    for (int line = 0; line < lines.Length && line < 2; line++)
                        mFont.tahoma_7b_dark.drawString(g, lines[line], textX, rowRect.Y + 15 + line * 11, mFont.LEFT);
                    if (message.time > 0L)
                    {
                        int ago = (int)System.Math.Max(0L, mSystem.currentTimeMillis() / 1000L - message.time);
                        mFont.tahoma_7_grey.drawString(g, NinjaUtil.getTimeAgo(ago) + " " + mResources.ago,
                            rowRect.Right - 4, rowRect.Bottom - 12, mFont.RIGHT);
                    }
                    PaintClanMessageOptions(g, message, rowRect);
                }
            }
        }

        private static string GetClanMessageText(ClanMessage message)
        {
            if (message == null) return string.Empty;
            if (message.type == 1)
                return mResources.request_pea + " (" + message.recieve + "/" + message.maxCap + ")";
            if (message.type == 2) return mResources.request_join_clan;
            if (message.chat == null || message.chat.Length == 0) return string.Empty;
            return string.Join(" ", message.chat);
        }

        private void PaintClanMessageOptions(mGraphics g, ClanMessage message, UiRect rowRect)
        {
            int visibleOptionCount = GetVisibleClanMessageOptionCount(message);
            if (visibleOptionCount == 0) return;
            int buttonWidth = 37;
            int startX = rowRect.Right - 3 - visibleOptionCount * buttonWidth;
            for (int i = 0; i < visibleOptionCount; i++)
            {
                UiRect rect = new UiRect(startX + i * buttonWidth, rowRect.Y + 4, buttonWidth - 2, 22);
                g.setColor(0xE99A00);
                g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
                g.setColor(0xF6C13A);
                g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);
                mFont.tahoma_7b_dark.drawString(g,
                    TruncateString(mFont.tahoma_7b_dark, message.option[i], rect.Width - 3),
                    rect.X + rect.Width / 2, rect.Y + 6, mFont.CENTER);
            }
        }

        private static bool IsOwnClanMessage(ClanMessage message)
        {
            Char me = Char.myCharz();
            if (message == null || me == null) return false;
            if (message.playerId == me.charID) return true;
            return !string.IsNullOrEmpty(me.cName)
                && string.Equals(message.playerName, me.cName, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetVisibleClanMessageOptionCount(ClanMessage message)
        {
            if (message == null || message.option == null || message.option.Length == 0) return 0;
            if (message.type == 1 && (IsOwnClanMessage(message) || message.recieve >= message.maxCap)) return 0;
            if (message.type == 4 && IsOwnClanMessage(message)) return 0;
            Char me = Char.myCharz();
            if (message.type == 2 && (me == null || me.role != 0 || IsOwnClanMessage(message))) return 0;
            return message.option.Length;
        }

        private static void PaintClanMemberAvatar(mGraphics g, Member member, UiRect rect)
        {
            if (member == null) return;
            using (UiRenderState.Push(g, rect, clip: true))
            {
                if (member.headICON >= 0)
                {
                    SmallImage.drawSmallImage(g, member.headICON, rect.X + rect.Width / 2,
                        rect.Y + rect.Height / 2, 0, StaticObj.VCENTER_HCENTER);
                    return;
                }
                int head = member.head;
                if (Char.myCharz() != null && member.ID == Char.myCharz().charID) head = Char.myCharz().head;
                if (GameScr.parts == null || head < 0 || head >= GameScr.parts.Length || GameScr.parts[head] == null)
                    return;
                Part part = GameScr.parts[head];
                int frame = Char.CharInfo[0][0][0];
                if (part.pi == null || frame < 0 || frame >= part.pi.Length || part.pi[frame] == null) return;
                SmallImage.drawSmallImage(g, part.pi[frame].id,
                    rect.X + rect.Width / 2 + Char.CharInfo[0][0][1] + part.pi[frame].dx - 3,
                    rect.Bottom, 0, mGraphics.LEFT | mGraphics.BOTTOM);
            }
        }

        private void PaintClanFunctionTabs(mGraphics g)
        {
            for (int i = 0; i < _clanFunctionRects.Length; i++)
            {
                bool active = i == GetClanFunctionForView(_selectedClanView);
                bool focused = _clanFocusArea == ClanFocusFunctions && i == _selectedClanFunction;
                PaintClanButton(g, _clanFunctionRects[i], ClanFunctionNames[i], active, focused);
            }
        }

        private static void PaintClanHeader(mGraphics g, UiRect rect, string text, bool red)
        {
            g.setColor(red ? 0x7B241D : 0x765018);
            g.fillRect(rect.X + 1, rect.Y + 2, System.Math.Max(1, rect.Width - 1),
                System.Math.Max(1, rect.Height - 1), 4);
            g.setColor(red ? 0xD83A2F : 0xE99A00);
            g.fillRect(rect.X, rect.Y, rect.Width, System.Math.Max(1, rect.Height - 2), 4);
            g.setColor(red ? 0xF07D66 : 0xF6C13A);
            g.fillRect(rect.X + 3, rect.Y + 1, System.Math.Max(1, rect.Width - 6), 1);
            (red ? mFont.tahoma_7b_white : mFont.tahoma_7b_dark).drawString(g, text,
                rect.X + rect.Width / 2, rect.Y + 5, mFont.CENTER);
        }

        private static void PaintClanSurface(mGraphics g, UiRect rect, int fill)
        {
            g.setColor(0x9A896F);
            g.fillRect(rect.X + 1, rect.Y + 1, System.Math.Max(1, rect.Width - 1),
                System.Math.Max(1, rect.Height - 1), 5);
            g.setColor(fill);
            g.fillRect(rect.X, rect.Y, System.Math.Max(1, rect.Width - 1),
                System.Math.Max(1, rect.Height - 2), 5);
            g.setColor(0xF8F1E6);
            g.fillRect(rect.X + 3, rect.Y + 1, System.Math.Max(1, rect.Width - 7), 1);
        }

        private static void PaintClanButton(mGraphics g, UiRect rect, string text, bool active, bool focused)
        {
            bool hovered = Main.isPC && rect.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
            int fill = active ? 0x64C70D : 0xE99A00;
            if (focused || hovered) fill = active ? 0x79DB20 : 0xF8B315;
            g.setColor(0x765018);
            g.fillRect(rect.X + 1, rect.Y + 2, System.Math.Max(1, rect.Width - 1),
                System.Math.Max(1, rect.Height - 1), 4);
            g.setColor(fill);
            g.fillRect(rect.X, rect.Y, rect.Width, System.Math.Max(1, rect.Height - 2), 4);
            g.setColor(active ? 0xD5FF70 : 0xFFE075);
            g.fillRect(rect.X + 2, rect.Y + 1, System.Math.Max(1, rect.Width - 4), 1);
            if (focused)
            {
                g.setColor(0xFFF2A8);
                g.drawRect(rect.X + 1, rect.Y + 1, System.Math.Max(1, rect.Width - 3),
                    System.Math.Max(1, rect.Height - 5));
            }
            mFont.tahoma_7b_dark.drawString(g, text, rect.X + rect.Width / 2,
                rect.Y + (rect.Height - mFont.tahoma_7b_dark.getHeight()) / 2 - 1, mFont.CENTER);
        }

        private static void PaintClanScrollbar(mGraphics g, ScrollViewAdapter adapter, UiRect viewport)
        {
            if (adapter == null || adapter.ScrollLimit <= 0 || viewport.IsEmpty) return;
            int trackHeight = System.Math.Max(1, viewport.Height - 8);
            int totalHeight = viewport.Height + adapter.ScrollLimit;
            int thumbHeight = System.Math.Max(12, viewport.Height * trackHeight / System.Math.Max(1, totalHeight));
            thumbHeight = System.Math.Min(trackHeight, thumbHeight);
            int travel = trackHeight - thumbHeight;
            int thumbY = viewport.Y + 4 + adapter.ScrollY * travel / System.Math.Max(1, adapter.ScrollLimit);
            int x = viewport.Right - 4;
            g.setColor(0xB9AA92);
            g.fillRect(x, viewport.Y + 4, 2, trackHeight, 2);
            g.setColor(0xE89A08);
            g.fillRect(x - 1, thumbY, 4, thumbHeight, 3);
            g.setColor(0xFFE17A);
            g.fillRect(x, thumbY + 1, 1, System.Math.Max(1, thumbHeight - 2));
        }

        private void PaintClanMembers(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xDED1BB);
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            MyVector members = GetClanMembers();
            using (UiRenderState.Push(g, _clanRightBodyRect, clip: true))
            {
                for (int i = 0; i < members.size(); i++)
                {
                    Member member = members.elementAt(i) as Member;
                    if (member == null) continue;
                    int y = _clanRightBodyRect.Y + i * ClanMemberRowHeight - scrollY;
                    if (y + ClanMemberRowHeight <= _clanRightBodyRect.Y || y >= _clanRightBodyRect.Bottom) continue;
                    bool focused = _clanFocusArea == ClanFocusContent && _selectedClanRow == i;
                    UiRect card = new UiRect(_clanRightBodyRect.X + 3, y + 2,
                        _clanRightBodyRect.Width - 9, ClanMemberRowHeight - 4);
                    g.setColor(focused ? 0xF8CD63 : 0xB9AA93);
                    g.fillRect(card.X, card.Y + 1, card.Width, card.Height, 4);
                    g.setColor(focused ? 0xFFF0B0 : 0xF6F3EE);
                    g.fillRect(card.X, card.Y, card.Width, System.Math.Max(1, card.Height - 1), 4);
                    UiRect avatar = new UiRect(card.X + 2, card.Y + 1, 28, card.Height - 2);
                    g.setColor(0xB7A489);
                    g.fillRect(avatar.X, avatar.Y, avatar.Width, avatar.Height, 3);
                    PaintClanMemberAvatar(g, member, avatar);
                    int textX = avatar.Right + 4;
                    mFont nameFont = member.role == 0 ? mFont.tahoma_7b_red
                        : member.role == 1 ? mFont.tahoma_7b_green : mFont.tahoma_7b_blue;
                    nameFont.drawString(g, TruncateString(nameFont, member.name ?? string.Empty,
                        _clanRightBodyRect.Width - 74), textX, y + 2, mFont.LEFT);
                    mFont.tahoma_7_grey.drawString(g, mResources.power + ": " + (member.powerPoint ?? "0"),
                        textX, y + 14, mFont.LEFT);
                    mFont.tahoma_7b_dark.drawString(g, member.clanPoint.ToString(),
                        card.Right - 15, y + 7, mFont.RIGHT);
                    SmallImage.drawSmallImage(g, 7223, card.Right - 7,
                        y + ClanMemberRowHeight / 2, 0, StaticObj.VCENTER_HCENTER);
                }
            }
        }

        private static List<string> BuildClanInfoLines()
        {
            List<string> lines = new List<string>();
            Clan clan = Char.myCharz() != null ? Char.myCharz().clan : null;
            if (clan == null) return lines;
            lines.Add("Thông tin chung:");
            lines.Add("- Tên: " + (clan.name ?? string.Empty));
            lines.Add("- Khẩu hiệu: " + (clan.slogan ?? string.Empty));
            lines.Add("- Thành viên: " + clan.currMember + "/" + clan.maxMember);
            lines.Add("- Bang chủ: " + (clan.leaderName ?? string.Empty));
            lines.Add("Clan Value:");
            if (!ClanValue.isReady(clan.ID)) lines.Add("- Đang tải giá trị bang...");
            else if (!ClanValue.current.enabled) lines.Add("- Tính năng đang tạm khóa");
            else
            {
                lines.Add("- Tổng: " + Res.formatNumber(ClanValue.current.totalValue));
                lines.Add("- Cấp bang: " + Res.formatNumber(ClanValue.current.clanLevelScore));
                lines.Add("- Tiềm năng đã dùng: " + Res.formatNumber(ClanValue.current.spentPotentialScore));
                lines.Add("- Cấp cây: " + Res.formatNumber(ClanValue.current.treeLevelScore));
                lines.Add("- Thành tích: " + Res.formatNumber(ClanValue.current.achievementScore));
                lines.Add("- Hoạt động tuần: " + Res.formatNumber(ClanValue.current.weeklyActivityScore));
            }
            lines.Add("Diện mạo Cây bang:");
            if (!ClanAppearance.isReady(clan.ID)) lines.Add("- Đang tải diện mạo...");
            else if (!ClanAppearance.current.enabled) lines.Add("- Tính năng đang tạm khóa");
            else
            {
                string appearanceTitle = !string.IsNullOrEmpty(ClanAppearance.current.title)
                    ? ClanAppearance.current.title : ClanAppearance.current.tierName;
                lines.Add("- Bậc hiện tại: " + appearanceTitle);
                if (ClanAppearance.current.nextTierId >= 0)
                {
                    lines.Add("- Kế tiếp: " + ClanAppearance.current.nextTierName);
                    lines.Add("- Còn cấp bang/cây: " + ClanAppearance.current.remainingClanLevels
                        + "/" + ClanAppearance.current.remainingTreeLevels);
                    lines.Add("- Còn Clan Value: " + Res.formatNumber(ClanAppearance.current.remainingClanValue));
                }
                else lines.Add("- Đã đạt bậc cao nhất");
            }
            lines.Add("Tài sản:");
            lines.Add("- Capsule bang: " + Res.formatNumber(ClanTreasury.current.capsule));
            lines.Add("- Vàng bang: " + Res.formatNumber(ClanTreasury.current.gold));
            lines.Add("- Ngọc bang: " + Res.formatNumber(ClanTreasury.current.gem));
            lines.Add("- Cống hiến: " + Res.formatNumber(ClanTreasury.current.contribution));
            lines.Add("Thuộc tính:");
            if (!ClanProgression.isReady(clan.ID)) lines.Add("- Đang tải chỉ số tiềm năng bang...");
            else
            {
                for (int branch = 0; branch < ClanProgression.BRANCH_COUNT; branch++)
                {
                    lines.Add("- " + ClanPotentialNames[branch] + ": +"
                        + ClanProgression.effectPercentText(branch) + "%");
                }
            }
            lines.Add("Buff bang:");
            for (int line = 0; line < ClanProgression.BUFF_COUNT; line++)
                lines.Add("- " + ClanProgression.buffStatusText(GetClanBuffDisplayType(line)));
            return lines;
        }

        private void PaintClanInfo(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xE8DDCC);
            UiRect viewport = GetClanScrollableBodyRect();
            List<string> lines = BuildClanInfoLines();
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            bool inBuffSection = false;
            int buffLine = 0;
            using (UiRenderState.Push(g, viewport, clip: true))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    int y = viewport.Y + i * 13 - scrollY;
                    if (y + 13 <= viewport.Y || y >= viewport.Bottom) continue;
                    string line = lines[i];
                    bool heading = line.EndsWith(":") && !line.StartsWith("-");
                    if (heading)
                    {
                        inBuffSection = line == "Buff bang:";
                        mFont.tahoma_7b_yellow.drawString(g,
                            TruncateString(mFont.tahoma_7b_yellow, line, viewport.Width - 8),
                            viewport.X + 3, y, mFont.LEFT);
                        continue;
                    }
                    if (inBuffSection && buffLine < ClanProgression.BUFF_COUNT)
                    {
                        int type = GetClanBuffDisplayType(buffLine++);
                        mFont buffFont = GetClanBuffFont(type);
                        string status = TruncateString(buffFont, line, viewport.Width - 8);
                        PaintClanBuffStatus(g, viewport, y, status, buffFont, type,
                            GetClanBuffColor(type), ClanProgression.isBuffActive(type));
                        continue;
                    }
                    mFont font = line.StartsWith("- Tổng:") ? mFont.tahoma_7b_yellow : mFont.tahoma_7b_dark;
                    string display = TruncateString(font, line, viewport.Width - 8);
                    if (line.StartsWith("- Bậc hiện tại:") && ClanAppearance.current.enabled)
                        font.drawStringColor(g, display, viewport.X + 3, y,
                            mFont.LEFT, ClanAppearance.current.accentRgb);
                    else font.drawString(g, display, viewport.X + 3, y, mFont.LEFT);
                }
            }
        }

        private static void PaintClanBuffStatus(mGraphics g, UiRect viewport, int y,
            string status, mFont font, int type, int color, bool active)
        {
            int textX = viewport.X + 3;
            if (active && NeedsClanBuffContrastBackground(type))
            {
                int chipWidth = System.Math.Min(viewport.Width - 5, font.getWidth(status) + 6);
                g.setColor(0x554335);
                g.fillRect(textX - 2, y, chipWidth, 12, 3);
            }
            if (active) font.drawStringColor(g, status, textX, y, mFont.LEFT, color);
            else font.drawString(g, status, textX, y, mFont.LEFT);
        }

        private static bool NeedsClanBuffContrastBackground(int type)
        {
            return type == 2 || type == 5;
        }

        private static int GetClanBuffDisplayType(int line)
        {
            if (line < 3) return line;
            if (line == 3) return 4;
            if (line == 4) return 3;
            return 5;
        }

        private static int GetClanBuffColor(int type)
        {
            if (type == 0) return 0xE00000;
            if (type == 1) return 0x0031E0;
            if (type == 2) return 0xFFFFFF;
            if (type == 3) return 0xDE00BA;
            if (type == 4) return 0x078700;
            return 0xFFF500;
        }

        private static mFont GetClanBuffFont(int type)
        {
            if (!ClanProgression.isBuffActive(type)) return mFont.tahoma_7b_dark;
            if (type == 0) return mFont.tahoma_7_red;
            if (type == 1) return mFont.tahoma_7_blue;
            if (type == 2) return mFont.tahoma_7_white;
            if (type == 3) return mFont.tahoma_7_blue;
            if (type == 4) return mFont.tahoma_7_green;
            return mFont.tahoma_7_yellow;
        }

        private void PaintClanPotential(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xDED1BB);
            Char me = Char.myCharz();
            if (me == null || me.clan == null || !ClanProgression.isReady(me.clan.ID))
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải tiến trình bang...",
                    _clanRightBodyRect.X + _clanRightBodyRect.Width / 2,
                    _clanRightBodyRect.Y + _clanRightBodyRect.Height / 2, mFont.CENTER);
                return;
            }
            mFont.tahoma_7_grey.drawString(g,
                "Số điểm còn lại: " + ClanProgression.current.unspentPoints,
                _clanRightBodyRect.X + 7, _clanRightBodyRect.Y + 3, mFont.LEFT);
            UiRect listRect = GetClanScrollableBodyRect();
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            using (UiRenderState.Push(g, listRect, clip: true))
            {
                for (int branch = 0; branch < ClanProgression.BRANCH_COUNT; branch++)
                {
                    int y = listRect.Y + branch * ClanPotentialRowHeight - scrollY;
                    if (y + ClanPotentialRowHeight <= listRect.Y || y >= listRect.Bottom) continue;
                    bool focused = _clanFocusArea == ClanFocusContent && _selectedClanRow == branch;
                    UiRect card = new UiRect(_clanRightBodyRect.X + 3, y + 2,
                        _clanRightBodyRect.Width - 9, ClanPotentialRowHeight - 4);
                    g.setColor(focused ? 0xF8CD63 : 0xB9AA93);
                    g.fillRect(card.X, card.Y + 1, card.Width, card.Height, 4);
                    g.setColor(focused ? 0xFFF0B0 : 0xEFE5D6);
                    g.fillRect(card.X, card.Y, card.Width, System.Math.Max(1, card.Height - 1), 4);
                    mFont.tahoma_7b_dark.drawString(g, ClanPotentialNames[branch],
                        card.X + 5, y + 3, mFont.LEFT);
                    mFont.tahoma_7_grey.drawString(g, ClanProgression.effectText(branch),
                        card.X + 5, y + 17, mFont.LEFT);
                    UiRect valueRect = new UiRect(card.Right - 75, y + 4, 39, 25);
                    g.setColor(0xF4EFE8);
                    g.fillRect(valueRect.X, valueRect.Y, valueRect.Width, valueRect.Height, 3);
                    mFont.tahoma_7b_dark.drawString(g, ClanProgression.rank(branch).ToString(),
                        valueRect.X + valueRect.Width / 2, valueRect.Y + 7, mFont.CENTER);
                    UiRect plusRect = new UiRect(card.Right - 29, y + 4, 27, 25);
                    _clanPotentialRects[branch] = plusRect;
                    bool canAdd = me.role == 0 && ClanProgression.current.unspentPoints > 0
                        && ClanProgression.rank(branch) < 20;
                    g.setColor(canAdd ? 0xFF4A17 : 0x9B8F82);
                    g.fillRect(plusRect.X + 10, plusRect.Y + 2, 7, 21);
                    g.fillRect(plusRect.X + 3, plusRect.Y + 9, 21, 7);
                }
            }
        }

        private void PaintClanTreasury(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xC4B397);
            if (!_clanStorageLoaded || Char.myCharz() == null || Char.myCharz().arrItemBox == null)
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải kho bang...",
                    _clanRightBodyRect.X + _clanRightBodyRect.Width / 2,
                    _clanRightBodyRect.Y + _clanRightBodyRect.Height / 2, mFont.CENTER);
                return;
            }
            Item[] storage = Char.myCharz().arrItemBox;
            const int gap = 2;
            int cellWidth = System.Math.Max(1,
                (_clanRightBodyRect.Width - gap * (ClanStorageColumns + 1)) / ClanStorageColumns);
            int cellHeight = ClanStorageRowHeight - gap;
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            using (UiRenderState.Push(g, _clanRightBodyRect, clip: true))
            {
                for (int slot = 0; slot < storage.Length; slot++)
                {
                    int row = slot / ClanStorageColumns;
                    int column = slot % ClanStorageColumns;
                    int x = _clanRightBodyRect.X + gap + column * (cellWidth + gap);
                    int y = _clanRightBodyRect.Y + gap + row * ClanStorageRowHeight - scrollY;
                    if (y + cellHeight <= _clanRightBodyRect.Y || y >= _clanRightBodyRect.Bottom) continue;
                    PaintInventoryItemCell(g, storage[slot], new UiRect(x, y, cellWidth, cellHeight),
                        slot == _selectedClanStorageSlot, 0xB7A489);
                }
            }
        }

        private void PaintClanUpgrade(mGraphics g)
        {
            PaintClanSurface(g, _clanRightBodyRect, 0xE8DDCC);
            Char me = Char.myCharz();
            int x = _clanRightBodyRect.X + 10;
            int y = _clanRightBodyRect.Y + 7;
            if (me == null || me.clan == null || !ClanProgression.isReady(me.clan.ID))
            {
                mFont.tahoma_7_grey.drawString(g, "Đang tải tiến trình bang...", x, y, mFont.LEFT);
                return;
            }
            ClanProgression progression = ClanProgression.current;
            mFont.tahoma_7b_dark.drawString(g, "Cấp bang: " + progression.level, x, y, mFont.LEFT);
            mFont.tahoma_7b_dark.drawString(g, "EXP: " + Res.formatNumber(progression.exp) + "/"
                + Res.formatNumber(progression.expRequired), x, y += 15, mFont.LEFT);
            PaintClanProgressBar(g, new UiRect(x, y + 13, _clanRightBodyRect.Width - 20, 6),
                progression.exp, progression.expRequired, 0x65C70D);
            y += 20;
            mFont.tahoma_7_green2.drawString(g, "Capsule bang: " + Res.formatNumber(ClanTreasury.current.capsule)
                + "/" + Res.formatNumber(progression.capsuleRequired), x, y, mFont.LEFT);
            mFont.tahoma_7_yellow.drawString(g, "Vàng: " + Res.formatNumber(ClanTreasury.current.gold)
                + "/" + Res.formatNumber(progression.goldRequired), x, y += 14, mFont.LEFT);
            mFont.tahoma_7_green2.drawString(g, "Ngọc: " + Res.formatNumber(ClanTreasury.current.gem)
                + "/" + Res.formatNumber(progression.gemRequired), x, y += 14, mFont.LEFT);
            PaintClanButton(g, _clanUpgradeButtonRect, "Nâng cấp", true,
                _clanFocusArea == ClanFocusContent);
        }

        private static void PaintClanProgressBar(mGraphics g, UiRect rect, long value, long maximum, int fill)
        {
            g.setColor(0x9A896F);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height, 3);
            if (maximum <= 0L || value <= 0L) return;
            long clamped = System.Math.Min(value, maximum);
            int width = (int)(clamped * System.Math.Max(1, rect.Width - 2) / maximum);
            g.setColor(fill);
            g.fillRect(rect.X + 1, rect.Y + 1, System.Math.Max(1, width),
                System.Math.Max(1, rect.Height - 2), 2);
        }

        private void PaintClanHistory(mGraphics g)
        {
            UiRect historyBody = _clanRightBodyRect;
            PaintClanSurface(g, historyBody, 0xDED1BB);
            int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
            int visibleIndex = 0;
            using (UiRenderState.Push(g, historyBody, clip: true))
            {
                for (int i = 0; i < ClanTreasury.current.ledger.size(); i++)
                {
                    ClanLedgerEntry entry = ClanTreasury.current.ledger.elementAt(i) as ClanLedgerEntry;
                    if (!ClanTreasury.isPlayerContributionEntry(entry)) continue;
                    int y = historyBody.Y + visibleIndex * ClanLedgerRowHeight - scrollY;
                    visibleIndex++;
                    if (y + ClanLedgerRowHeight <= historyBody.Y || y >= historyBody.Bottom) continue;
                    UiRect card = new UiRect(historyBody.X + 3, y + 2,
                        historyBody.Width - 9, ClanLedgerRowHeight - 4);
                    g.setColor(0xB9AA93);
                    g.fillRect(card.X, card.Y + 1, card.Width, card.Height, 4);
                    g.setColor(0xF8F6F2);
                    g.fillRect(card.X, card.Y, card.Width, System.Math.Max(1, card.Height - 1), 4);
                    Member actor = FindClanMember((int)entry.actorId);
                    UiRect avatar = new UiRect(card.X + 2, card.Y + 1, 32, card.Height - 2);
                    g.setColor(0xB7A489);
                    g.fillRect(avatar.X, avatar.Y, avatar.Width, avatar.Height, 3);
                    PaintClanMemberAvatar(g, actor, avatar);
                    bool gem = entry.currencyType == ClanTreasury.CURRENCY_GEM;
                    int textX = avatar.Right + 5;
                    mFont.tahoma_7b_dark.drawString(g,
                        TruncateString(mFont.tahoma_7b_dark, (entry.actorName ?? "Người chơi")
                            + " đã góp " + (gem ? "ngọc" : "vàng"), historyBody.Width - 91),
                        textX, y + 4, mFont.LEFT);
                    mFont.tahoma_7_grey.drawString(g, FormatClanLedgerDate(entry.createdAt),
                        textX, y + 18, mFont.LEFT);
                    mFont amountFont = gem ? mFont.tahoma_7b_green2 : mFont.tahoma_7b_yellow;
                    amountFont.drawString(g, "+" + Res.formatNumber(entry.amount),
                        card.Right - 16, y + 10, mFont.RIGHT);
                    Image icon = gem ? Panel.imgLuong : Panel.imgXu;
                    if (icon != null) g.drawImage(icon, card.Right - 7,
                        y + ClanLedgerRowHeight / 2, mGraphics.HCENTER | mGraphics.VCENTER);
                }
            }
            if (visibleIndex == 0)
                mFont.tahoma_7_grey.drawString(g, "Chưa có đóng góp vàng hoặc ngọc.",
                    historyBody.X + historyBody.Width / 2, historyBody.Y + historyBody.Height / 2,
                    mFont.CENTER);
        }

        private static string FormatClanLedgerDate(long seconds)
        {
            try
            {
                DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddSeconds(seconds).AddHours(7.0);
                return date.ToString("HH'h'mm - dd/MM/yyyy");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private int GetClanSideActionCount()
        {
            if (_selectedClanView == ClanViewTreasury) return 3;
            if (_selectedClanView == ClanViewHistory) return 4;
            if (_selectedClanView == ClanViewMembers)
                return Char.myCharz() != null && Char.myCharz().role == 0 ? 3 : 1;
            return 0;
        }

        private string GetClanSideActionLabel(int index)
        {
            if (_selectedClanView == ClanViewTreasury || _selectedClanView == ClanViewHistory)
            {
                if (index == 0) return "Góp vàng";
                if (index == 1) return "Góp ngọc";
                if (index == 2) return "Lịch sử";
                return "Quay lại";
            }
            if (Char.myCharz() != null && Char.myCharz().role == 0)
            {
                if (index == 0) return "Khẩu hiệu";
                if (index == 1) return "Biểu tượng";
                return "Rời bang";
            }
            return "Rời bang";
        }

        private void PaintClanSideActions(mGraphics g)
        {
            int count = GetClanSideActionCount();
            for (int i = 0; i < count; i++)
                PaintClanButton(g, _clanSideActionRects[i], GetClanSideActionLabel(i),
                    _selectedClanView == ClanViewHistory && i == 2,
                    _clanFocusArea == ClanFocusSideActions && _selectedClanSideAction == i);
        }

        private bool HandleClanPointerInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                for (int i = 0; i < _clanFunctionRects.Length; i++)
                {
                    UiRect rect = _clanFunctionRects[i];
                    if (_selectedClanView != ClanViewHistory
                        && GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height))
                    {
                        GameCanvas.clearAllPointerEvent();
                        _keyboardFocus = KeyboardFocusContent;
                        _clanFocusArea = ClanFocusFunctions;
                        _selectedClanFunction = i;
                        ActivateClanFunction(i);
                        return true;
                    }
                }
                int sideCount = GetClanSideActionCount();
                for (int i = 0; i < sideCount; i++)
                {
                    UiRect rect = _clanSideActionRects[i];
                    if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                    GameCanvas.clearAllPointerEvent();
                    _keyboardFocus = KeyboardFocusContent;
                    _clanFocusArea = ClanFocusSideActions;
                    _selectedClanSideAction = i;
                    ActivateClanSideAction(i);
                    return true;
                }
                if (GameCanvas.isPointer(_clanShareRect.X, _clanShareRect.Y,
                    _clanShareRect.Width, _clanShareRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    Service.gI().shareClanLocation();
                    return true;
                }
                if (GameCanvas.isPointer(_clanSendRect.X, _clanSendRect.Y,
                    _clanSendRect.Width, _clanSendRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    SendClanChat();
                    return true;
                }
                if (_clanChatField != null && GameCanvas.isPointer(_clanChatField.x, _clanChatField.y,
                    _clanChatField.width, _clanChatField.height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanFocusArea = ClanFocusChat;
                    _clanChatFocused = true;
                    _clanChatField.setFocusWithKb(true);
                    return true;
                }
                if (_selectedClanView == ClanViewUpgrade
                    && GameCanvas.isPointer(_clanUpgradeButtonRect.X, _clanUpgradeButtonRect.Y,
                        _clanUpgradeButtonRect.Width, _clanUpgradeButtonRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanFocusArea = ClanFocusContent;
                    ActivateClanContent(0);
                    return true;
                }
                if (_selectedClanView == ClanViewPotential)
                {
                    for (int branch = 0; branch < ClanProgression.BRANCH_COUNT; branch++)
                    {
                        UiRect rect = _clanPotentialRects[branch];
                        if (!GameCanvas.isPointer(rect.X, rect.Y, rect.Width, rect.Height)) continue;
                        GameCanvas.clearAllPointerEvent();
                        _clanFocusArea = ClanFocusContent;
                        _selectedClanRow = branch;
                        ActivateClanContent(branch);
                        return true;
                    }
                }
            }

            if (_leftScrollAdapter != null && _leftScrollAdapter.UpdateKey(_inputContext, out int chatIndex))
            {
                _clanChatFocused = false;
                _clanChatField?.setFocus(false);
                if (chatIndex >= 0) ActivateClanMessage(chatIndex);
                return true;
            }
            if (_rightScrollAdapter != null && _rightScrollAdapter.UpdateKey(_inputContext, out int contentIndex))
            {
                _clanFocusArea = ClanFocusContent;
                if (contentIndex >= 0)
                {
                    if (_selectedClanView == ClanViewTreasury)
                    {
                        int column = System.Math.Max(0, System.Math.Min(ClanStorageColumns - 1,
                            (GameCanvas.px - _clanRightBodyRect.X) * ClanStorageColumns
                            / System.Math.Max(1, _clanRightBodyRect.Width)));
                        _selectedClanRow = contentIndex;
                        ActivateClanContent(contentIndex * ClanStorageColumns + column);
                    }
                    else
                    {
                        _selectedClanRow = contentIndex;
                        if (_selectedClanView != ClanViewPotential)
                            ActivateClanContent(contentIndex);
                    }
                }
                return true;
            }

            int ascii = GameCanvas.keyAsciiPress;
            if (HandleClanTextBackspace(_clanChatField, _clanChatFocused)) return true;
            if (_clanChatFocused && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _clanChatField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
                return true;
            }
            return false;
        }

        private void ActivateClanFunction(int index)
        {
            if (index == 0) SelectClanView(ClanViewMembers);
            else if (index == 1) SelectClanView(ClanViewInfo);
            else if (index == 2) OpenClanTreasury();
            else if (index == 3) SelectClanView(ClanViewPotential);
            else if (index == 4)
            {
                Service.gI().clanMessage(1, null, -1);
                GameScr.info1.addInfo("Đã gửi xin đậu trong bang.", 0);
                SoundMn.gI().panelClick();
            }
            else if (index == 5) SelectClanView(ClanViewUpgrade);
        }

        private void ActivateClanContent(int index)
        {
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            if (_selectedClanView == ClanViewPotential)
            {
                if (!ClanProgression.isReady(me.clan.ID))
                    GameScr.info1.addInfo("Đang tải tiến trình bang.", 0);
                else if (me.role != 0)
                    GameScr.info1.addInfo("Chỉ bang chủ được cộng điểm tiềm năng bang.", 0);
                else if (index >= 0 && index < ClanProgression.BRANCH_COUNT)
                    ClanProgression.allocate(index);
                return;
            }
            if (_selectedClanView == ClanViewUpgrade)
            {
                if (!ClanProgression.isReady(me.clan.ID))
                    GameScr.info1.addInfo("Đang tải tiến trình bang.", 0);
                else if (me.role != 0)
                    GameScr.info1.addInfo("Chỉ bang chủ được nâng cấp bang.", 0);
                else Service.gI().clanProgression(ClanProgression.REQUEST_UPGRADE);
                return;
            }
            if (_selectedClanView == ClanViewTreasury && _clanStorageLoaded
                && me.arrItemBox != null && index >= 0 && index < me.arrItemBox.Length)
            {
                if (me.arrItemBox[index] == null) return;
                if (_selectedClanStorageSlot == index)
                    Service.gI().clanItemStorageUse(index);
                else
                {
                    _selectedClanStorageSlot = index;
                    GameScr.info1.addInfo("Chọn lại để dùng " + me.arrItemBox[index].template.name + ".", 0);
                }
            }
        }

        private void ActivateClanMessage(int index)
        {
            if (index < 0 || index >= ClanMessage.vMessage.size()) return;
            ClanMessage message = ClanMessage.vMessage.elementAt(index) as ClanMessage;
            int visibleOptionCount = GetVisibleClanMessageOptionCount(message);
            if (visibleOptionCount == 0) return;
            if (message.type == 1)
                Service.gI().clanDonate(message.id);
            else if (message.type == 4)
                Service.gI().clanTree(ClanTree.REQUEST_HELP_WATER);
            else if (message.type == 2 && Char.myCharz().role == 0)
            {
                int optionWidth = 37;
                int rowRight = _clanChatListRect.Right - 2;
                int optionStart = rowRight - visibleOptionCount * optionWidth;
                int option = (GameCanvas.px - optionStart) / optionWidth;
                if (option >= 0 && option < visibleOptionCount)
                    Service.gI().joinClan(message.id, (sbyte)(option == 0 ? 1 : 0));
            }
        }

        private void ActivateClanSideAction(int index)
        {
            if (_selectedClanView == ClanViewTreasury || _selectedClanView == ClanViewHistory)
            {
                if (index == 0) OpenClanDialog(ClanInputDepositGold);
                else if (index == 1) OpenClanDialog(ClanInputDepositGem);
                else if (index == 2)
                {
                    _selectedClanView = ClanViewHistory;
                    _selectedClanFunction = 2;
                    ConfigureClanRects();
                    _rightScrollAdapter?.Reset();
                    Service.gI().clanTreasuryLedger(0L);
                    ConfigureScrollAdapters();
                    SoundMn.gI().panelClick();
                }
                else if (index == 3) OpenClanTreasury();
                return;
            }
            Char me = Char.myCharz();
            if (me == null || me.clan == null) return;
            if (me.role == 0)
            {
                if (index == 0) OpenClanDialog(ClanInputSlogan);
                else if (index == 1) OpenClanIconPicker();
                else if (index == 2) Service.gI().leaveClan();
            }
            else if (index == 0) Service.gI().leaveClan();
        }

        private void OpenClanIconPicker()
        {
            Panel panel = GameCanvas.panel;
            Close();
            if (panel != null)
            {
                panel.setTypeMain();
                panel.currentTabIndex = 3;
                panel.setTabClans();
                panel.show();
            }
            Service.gI().getClan(3, -1, null);
        }

        private static bool HandleClanTextBackspace(TField field, bool focused)
        {
            if (!Main.isPC || !focused || field == null || !GameCanvas.keyPressed[14]) return false;
            GameCanvas.keyPressed[14] = false;
            field.keyPressed(-8);
            return true;
        }

        private void SendClanChat()
        {
            string text = _clanChatField != null ? _clanChatField.getText().Trim() : string.Empty;
            if (text.Length == 0) return;
            Service.gI().clanMessage(0, text, -1);
            _clanChatField.setText(string.Empty);
            _clanChatFocused = false;
            _clanChatField.setFocus(false);
            SoundMn.gI().panelClick();
        }

        private void OpenClanDialog(int mode)
        {
            _clanDialogMode = mode;
            _clanDialogFocus = 0;
            _clanDialogField = new TField();
            _clanDialogField.name = mode == ClanInputSlogan ? "Nhập khẩu hiệu ..." : "Nhập tại đây ...";
            _clanDialogField.setIputType(mode == ClanInputSlogan ? TField.INPUT_TYPE_ANY : TField.INPUT_TYPE_NUMERIC);
            _clanDialogField.setMaxTextLenght(mode == ClanInputSlogan ? 80 : 18);
            ConfigureClanRects();
            _clanDialogField.setFocusWithKb(true);
            GameCanvas.keyAsciiPress = 0;
            GameCanvas.clearKeyPressed();
            SoundMn.gI().panelClick();
        }

        private void CloseClanDialog()
        {
            _clanDialogMode = ClanInputNone;
            if (_clanDialogField != null) _clanDialogField.setFocus(false);
            _clanDialogField = null;
            _clanDialogFocus = 0;
        }

        private void HandleClanDialogInput()
        {
            if (GameCanvas.isPointerJustRelease)
            {
                if (GameCanvas.isPointer(_clanDialogCloseRect.X, _clanDialogCloseRect.Y,
                    _clanDialogCloseRect.Width, _clanDialogCloseRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    CloseClanDialog();
                    return;
                }
                if (_clanDialogField != null && GameCanvas.isPointer(_clanDialogField.x, _clanDialogField.y,
                    _clanDialogField.width, _clanDialogField.height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanDialogFocus = 0;
                    _clanDialogField.setFocusWithKb(true);
                    return;
                }
                if (GameCanvas.isPointer(_clanDialogSubmitRect.X, _clanDialogSubmitRect.Y,
                    _clanDialogSubmitRect.Width, _clanDialogSubmitRect.Height))
                {
                    GameCanvas.clearAllPointerEvent();
                    _clanDialogFocus = 1;
                    SubmitClanDialog();
                    return;
                }
                GameCanvas.clearAllPointerEvent();
            }
            if (GameCanvas.keyAsciiPress == 27 || GameCanvas.keyPressed[12] || GameCanvas.keyPressed[13])
            {
                GameCanvas.keyAsciiPress = 0;
                GameCanvas.clearKeyPressed();
                CloseClanDialog();
                return;
            }
            if (HandleClanTextBackspace(_clanDialogField, _clanDialogFocus == 0)) return;
            if (GameCanvas.keyPressed[22] || GameCanvas.keyPressed[8])
            {
                ConsumeDirectionKeys(22, 8);
                _clanDialogFocus = 1;
                _clanDialogField?.setFocus(false);
                return;
            }
            if (GameCanvas.keyPressed[21] || GameCanvas.keyPressed[2])
            {
                ConsumeDirectionKeys(21, 2);
                _clanDialogFocus = 0;
                _clanDialogField?.setFocus(true);
                return;
            }
            if (GameCanvas.keyPressed[Main.isPC ? 25 : 5])
            {
                GameCanvas.clearKeyPressed();
                if (_clanDialogFocus == 0)
                {
                    _clanDialogFocus = 1;
                    _clanDialogField?.setFocus(false);
                }
                else SubmitClanDialog();
                return;
            }
            int ascii = GameCanvas.keyAsciiPress;
            if (_clanDialogFocus == 0 && ascii != 0 && ascii != 10 && ascii != 13)
            {
                _clanDialogField?.keyPressed(ascii);
                GameCanvas.keyAsciiPress = 0;
            }
        }

        private void SubmitClanDialog()
        {
            string value = _clanDialogField != null ? _clanDialogField.getText().Trim() : string.Empty;
            if (_clanDialogMode == ClanInputSlogan)
            {
                if (value.Length == 0)
                {
                    GameScr.info1.addInfo(mResources.clan_slogan_blank, 0);
                    return;
                }
                Service.gI().getClan(4, (sbyte)Char.myCharz().clan.imgID, value);
                CloseClanDialog();
                return;
            }
            if (!long.TryParse(value, out long amount) || amount <= 0L)
            {
                GameScr.info1.addInfo("Số lượng đóng góp không hợp lệ.", 0);
                return;
            }
            sbyte currency = _clanDialogMode == ClanInputDepositGold
                ? ClanTreasury.CURRENCY_GOLD : ClanTreasury.CURRENCY_GEM;
            Service.gI().clanTreasuryDeposit(currency, amount,
                DateTime.UtcNow.Ticks + "-custom-clan-" + Char.myCharz().charID);
            CloseClanDialog();
        }

        private void PaintClanDialog(mGraphics g)
        {
            int accent = GetClanDialogAccent();
            g.setColor(0x17110D, 0.48f);
            g.fillRect(0, 0, GameCanvas.w, GameCanvas.h + 1);
            g.setColor(0x3B2C22);
            g.fillRect(_clanDialogRect.X + 3, _clanDialogRect.Y + 4,
                _clanDialogRect.Width, _clanDialogRect.Height, 9);
            g.setColor(0xEFE4D5);
            g.fillRect(_clanDialogRect.X, _clanDialogRect.Y,
                _clanDialogRect.Width, _clanDialogRect.Height, 9);
            g.setColor(accent);
            g.fillRect(_clanDialogRect.X, _clanDialogRect.Y,
                _clanDialogRect.Width, 29, 9);
            g.setColor(0xFFF2B8);
            g.fillRect(_clanDialogRect.X + 5, _clanDialogRect.Y + 2,
                _clanDialogRect.Width - 10, 1);

            PaintClanDialogIcon(g, accent);
            string title = _clanDialogMode == ClanInputSlogan ? "Đổi khẩu hiệu bang"
                : (_clanDialogMode == ClanInputDepositGold ? "Góp vàng vào bang" : "Góp ngọc vào bang");
            string hint = _clanDialogMode == ClanInputSlogan ? "Tối đa 80 ký tự"
                : "Nhập số lượng muốn chuyển vào ngân quỹ bang";
            mFont.tahoma_7b_white.drawString(g, title, _clanDialogRect.X + 37,
                _clanDialogRect.Y + 8, mFont.LEFT);
            mFont.tahoma_7_grey.drawString(g, hint, _clanDialogRect.X + 25,
                _clanDialogRect.Y + 34, mFont.LEFT);

            g.setColor(0xB6A58D);
            g.fillRect(_clanDialogField.x - 3, _clanDialogField.y - 2,
                _clanDialogField.width + 6, _clanDialogField.height + 4, 6);
            g.setColor(0xFBF8F3);
            g.fillRect(_clanDialogField.x - 2, _clanDialogField.y - 1,
                _clanDialogField.width + 4, _clanDialogField.height + 2, 5);

            g.setColor(0x8C2D25);
            g.fillRect(_clanDialogCloseRect.X, _clanDialogCloseRect.Y,
                _clanDialogCloseRect.Width, _clanDialogCloseRect.Height, 5);
            g.setColor(0xFFD1C7);
            g.drawLine(_clanDialogCloseRect.X + 5, _clanDialogCloseRect.Y + 5,
                _clanDialogCloseRect.X + 12, _clanDialogCloseRect.Y + 12);
            g.drawLine(_clanDialogCloseRect.X + 12, _clanDialogCloseRect.Y + 5,
                _clanDialogCloseRect.X + 5, _clanDialogCloseRect.Y + 12);
            _clanDialogField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintClanDialogActionButton(g, accent,
                _clanDialogMode == ClanInputSlogan ? "Lưu khẩu hiệu" : "Xác nhận góp");
        }

        private int GetClanDialogAccent()
        {
            if (_clanDialogMode == ClanInputDepositGem) return 0x18A66A;
            if (_clanDialogMode == ClanInputSlogan) return 0xD44738;
            return 0xE99A00;
        }

        private void PaintClanDialogIcon(mGraphics g, int accent)
        {
            int centerX = _clanDialogRect.X + 20;
            int centerY = _clanDialogRect.Y + 14;
            Image icon = _clanDialogMode == ClanInputDepositGold ? Panel.imgXu
                : _clanDialogMode == ClanInputDepositGem ? Panel.imgLuong : null;
            if (icon != null)
            {
                g.drawImage(icon, centerX, centerY, mGraphics.HCENTER | mGraphics.VCENTER);
                return;
            }
            g.setColor(0xFFF4D2);
            g.fillRect(centerX - 7, centerY - 5, 14, 10, 3);
            g.fillRect(centerX - 4, centerY + 4, 4, 3, 1);
            g.setColor(accent);
            g.fillRect(centerX - 4, centerY - 2, 8, 1);
            g.fillRect(centerX - 4, centerY + 1, 6, 1);
        }

        private void PaintClanDialogActionButton(mGraphics g, int accent, string text)
        {
            bool focused = _clanDialogFocus == 1;
            bool hovered = Main.isPC && _clanDialogSubmitRect.Contains(GameCanvas.pxMouse, GameCanvas.pyMouse);
            g.setColor(0x765018);
            g.fillRect(_clanDialogSubmitRect.X + 1, _clanDialogSubmitRect.Y + 2,
                _clanDialogSubmitRect.Width, _clanDialogSubmitRect.Height - 1, 5);
            g.setColor(focused || hovered ? LightenClanDialogAccent(accent) : accent);
            g.fillRect(_clanDialogSubmitRect.X, _clanDialogSubmitRect.Y,
                _clanDialogSubmitRect.Width, _clanDialogSubmitRect.Height - 2, 5);
            g.setColor(0xFFF2B8);
            g.fillRect(_clanDialogSubmitRect.X + 3, _clanDialogSubmitRect.Y + 1,
                _clanDialogSubmitRect.Width - 6, 1);
            mFont.tahoma_7b_white.drawString(g, text,
                _clanDialogSubmitRect.X + _clanDialogSubmitRect.Width / 2,
                _clanDialogSubmitRect.Y + 7, mFont.CENTER);
        }

        private static int LightenClanDialogAccent(int color)
        {
            int red = System.Math.Min(255, ((color >> 16) & 0xFF) + 24);
            int green = System.Math.Min(255, ((color >> 8) & 0xFF) + 24);
            int blue = System.Math.Min(255, (color & 0xFF) + 24);
            return red << 16 | green << 8 | blue;
        }

        private void MoveClanHorizontalFocus(int direction)
        {
            if (_clanFocusArea == ClanFocusFunctions)
            {
                int column = _selectedClanFunction % 2;
                if (direction < 0 && column == 0)
                {
                    _keyboardFocus = KeyboardFocusMainTabs;
                    SoundMn.gI().panelClick();
                }
                else if (direction > 0 && column == 1)
                {
                    _clanFocusArea = ClanFocusContent;
                    if (_selectedClanView == ClanViewTreasury && _selectedClanStorageSlot < 0)
                        _selectedClanStorageSlot = 0;
                    SoundMn.gI().panelClick();
                }
                else
                {
                    _selectedClanFunction = System.Math.Max(0,
                        System.Math.Min(_clanFunctionRects.Length - 1, _selectedClanFunction + direction));
                    SoundMn.gI().panelClick();
                }
                return;
            }
            if (_clanFocusArea == ClanFocusContent)
            {
                if (_selectedClanView == ClanViewTreasury)
                {
                    Char me = Char.myCharz();
                    int slotCount = me != null && me.arrItemBox != null ? me.arrItemBox.Length : 0;
                    if (slotCount > 0)
                    {
                        if (_selectedClanStorageSlot < 0) _selectedClanStorageSlot = 0;
                        int column = _selectedClanStorageSlot % ClanStorageColumns;
                        int target = _selectedClanStorageSlot + direction;
                        if (direction < 0 && column == 0) _clanFocusArea = ClanFocusFunctions;
                        else if (direction > 0 && (column == ClanStorageColumns - 1 || target >= slotCount))
                        {
                            if (GetClanSideActionCount() > 0) _clanFocusArea = ClanFocusSideActions;
                        }
                        else
                        {
                            _selectedClanStorageSlot = target;
                            _selectedClanRow = target / ClanStorageColumns;
                            _rightScrollAdapter?.ScrollToIndex(_selectedClanRow);
                        }
                    }
                    else if (direction < 0) _clanFocusArea = ClanFocusFunctions;
                    else if (GetClanSideActionCount() > 0) _clanFocusArea = ClanFocusSideActions;
                }
                else if (direction < 0) _clanFocusArea = ClanFocusFunctions;
                else if (GetClanSideActionCount() > 0) _clanFocusArea = ClanFocusSideActions;
                SoundMn.gI().panelClick();
                return;
            }
            if (_clanFocusArea == ClanFocusSideActions && direction < 0)
            {
                _clanFocusArea = ClanFocusContent;
                SoundMn.gI().panelClick();
            }
        }

        private void MoveClanVerticalSelection(int direction)
        {
            if (_clanFocusArea == ClanFocusFunctions)
            {
                int target = _selectedClanFunction + direction * 2;
                _selectedClanFunction = System.Math.Max(0,
                    System.Math.Min(_clanFunctionRects.Length - 1, target));
            }
            else if (_clanFocusArea == ClanFocusSideActions)
            {
                int count = GetClanSideActionCount();
                if (count > 0) _selectedClanSideAction = (_selectedClanSideAction + direction + count) % count;
            }
            else if (_clanFocusArea == ClanFocusContent)
            {
                if (_selectedClanView == ClanViewTreasury)
                {
                    Char me = Char.myCharz();
                    int slotCount = me != null && me.arrItemBox != null ? me.arrItemBox.Length : 0;
                    if (slotCount > 0)
                    {
                        if (_selectedClanStorageSlot < 0) _selectedClanStorageSlot = 0;
                        int target = _selectedClanStorageSlot + direction * ClanStorageColumns;
                        if (target >= 0 && target < slotCount) _selectedClanStorageSlot = target;
                        _selectedClanRow = _selectedClanStorageSlot / ClanStorageColumns;
                        _rightScrollAdapter?.ScrollToIndex(_selectedClanRow);
                    }
                }
                else
                {
                    int count = GetClanContentItemCount();
                    if (count > 0)
                    {
                        _selectedClanRow = (_selectedClanRow + direction + count) % count;
                        _rightScrollAdapter?.ScrollToIndex(_selectedClanRow);
                    }
                }
            }
            SoundMn.gI().panelClick();
        }

        private void HandleClanConfirm()
        {
            if (_clanChatFocused)
            {
                SendClanChat();
                return;
            }
            if (_clanFocusArea == ClanFocusFunctions) ActivateClanFunction(_selectedClanFunction);
            else if (_clanFocusArea == ClanFocusSideActions) ActivateClanSideAction(_selectedClanSideAction);
            else if (_clanFocusArea == ClanFocusContent)
                ActivateClanContent(_selectedClanView == ClanViewTreasury
                    ? _selectedClanStorageSlot : _selectedClanRow);
        }

        private void PaintSkillTabContent(mGraphics g)
        {
            PaintSkillHeaders(g);
            PaintSkillListColumn(g);
            PaintSkillDetailColumn(g);
            if (_showSkillKeyPicker) PaintSkillKeyPicker(g);
            if (_showIntrinsicList || _showIntrinsicConfirmation) PaintIntrinsicSideButtons(g);
        }

        private void PaintSkillHeaders(mGraphics g)
        {
            PaintSkillHeader(g, _skillListHeaderRect, "Tiềm năng : " + NinjaUtil.getMoneys(Char.myCharz() != null ? Char.myCharz().cTiemNang : 0L));
            PaintSkillHeader(g, _skillDetailHeaderRect, _showIntrinsicList ? "Danh sách nội tại" : "Chi tiết");
        }

        private static void PaintSkillHeader(mGraphics g, UiRect rect, string text)
        {
            g.setColor(0xD93A2E);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(0xB52B23);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);
            mFont.tahoma_7b_white.drawString(g, text, rect.X + rect.Width / 2, rect.Y + 4, mFont.CENTER);
        }

        private void PaintSkillListColumn(mGraphics g)
        {
            g.setColor(0xF2F0ED);
            g.fillRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);
            g.setColor(0xC4B79B);
            g.drawRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);

            using (UiRenderState.Push(g, _leftColRect, clip: true))
            {
                int scrollY = _leftScrollAdapter != null ? _leftScrollAdapter.ScrollY : 0;
                int rowCount = GetSkillRowCount();
                for (int i = 0; i < rowCount; i++)
                {
                    int rowY = _leftColRect.Y + i * SkillRowHeight - scrollY;
                    if (rowY + SkillRowHeight <= _leftColRect.Y || rowY >= _leftColRect.Y + _leftColRect.Height) continue;
                    PaintSkillListRow(g, i, rowY);
                }
            }
        }

        private void PaintSkillListRow(mGraphics g, int row, int y)
        {
            bool selected = row == _selectedSkillRow;
            g.setColor(selected ? 0xFFD038 : 0xF2F0ED);
            g.fillRect(_leftColRect.X + 1, y, _leftColRect.Width - 2, SkillRowHeight - 1);
            g.setColor(0xC4B79B);
            g.drawLine(_leftColRect.X + 1, y + SkillRowHeight - 1, _leftColRect.X + _leftColRect.Width - 2, y + SkillRowHeight - 1);

            if (GameScr.imgSkill != null) g.drawImage(GameScr.imgSkill, _leftColRect.X + 3, y + 3, 0);
            if (row < PotentialStatRowCount) PaintPotentialListRow(g, row, y);
            else if (row == IntrinsicRowIndex) PaintIntrinsicListRow(g, y);
            else PaintTemplateSkillListRow(g, row - SkillTemplateStartRow, y);
        }

        private void PaintPotentialListRow(mGraphics g, int row, int y)
        {
            Char me = Char.myCharz();
            if (me == null) return;
            SmallImage.drawSmallImage(g, PotentialIcons[row], _leftColRect.X + 7, y + 7, 0, 0);

            int textX = _leftColRect.X + 41;
            int textWidth = _leftColRect.Width - 46;
            string valueSuffix = row == 4 ? "%" : string.Empty;
            string title = PotentialNames[row] + " : " + NinjaUtil.getMoneys(GetPotentialCurrentValue(me, row)) + valueSuffix;
            string subtitle = NinjaUtil.getMoneys(GetPotentialCost(me, row)) + " tiềm năng : Tăng "
                + GetPotentialIncreaseValue(me, row) + valueSuffix;
            mFont.tahoma_7b_blue.drawString(g, TruncateString(mFont.tahoma_7b_blue, title, textWidth), textX, y + 4, mFont.LEFT);
            mFont.tahoma_7_grey.drawString(g, TruncateString(mFont.tahoma_7_grey, subtitle, textWidth), textX, y + 19, mFont.LEFT);
        }

        private void PaintIntrinsicListRow(mGraphics g, int y)
        {
            if (Panel.specialInfo != null && Panel.spearcialImage >= 0)
                SmallImage.drawSmallImage(g, Panel.spearcialImage, _leftColRect.X + 7, y + 7, 0, 0);
            int textX = _leftColRect.X + 41;
            int textWidth = _leftColRect.Width - 46;
            mFont.tahoma_7b_blue.drawString(g, "Nội tại", textX, y + 4, mFont.LEFT);
            string summary = string.IsNullOrEmpty(Panel.specialInfo) ? "Chưa mở nội tại" : Panel.specialInfo;
            mFont.tahoma_7_grey.drawString(g, TruncateString(mFont.tahoma_7_grey, summary, textWidth), textX, y + 19, mFont.LEFT);
        }

        private void PaintTemplateSkillListRow(mGraphics g, int templateIndex, int y)
        {
            Char me = Char.myCharz();
            if (me == null || me.nClass == null || me.nClass.skillTemplates == null || templateIndex >= me.nClass.skillTemplates.Length) return;
            SkillTemplate template = me.nClass.skillTemplates[templateIndex];
            if (template == null) return;
            Skill skill = me.getSkill(template);
            SmallImage.drawSmallImage(g, template.iconId, _leftColRect.X + 7, y + 7, 0, 0);

            int textX = _leftColRect.X + 41;
            int textRight = _leftColRect.X + _leftColRect.Width - 5;
            int nameWidth = _leftColRect.Width - 85;
            if (skill != null)
            {
                mFont.tahoma_7b_blue.drawString(g, TruncateString(mFont.tahoma_7b_blue, template.name, nameWidth), textX, y + 4, mFont.LEFT);
                mFont.tahoma_7_blue.drawString(g, "Cấp " + skill.getDisplayLevel(), textRight, y + 4, mFont.RIGHT);
                int barWidth = System.Math.Min(54, _leftColRect.Width - 50);
                int progress = skill.curExp;
                if (progress < 0) progress = 0;
                if (progress > 1000) progress = 1000;
                g.setColor(0xC9D9AA);
                g.fillRect(textX, y + 22, barWidth, 7);
                g.setColor(0x00BE69);
                g.fillRect(textX, y + 22, progress * barWidth / 1000, 7);
            }
            else
            {
                mFont.tahoma_7b_green.drawString(g, TruncateString(mFont.tahoma_7b_green, template.name, _leftColRect.Width - 48), textX, y + 4, mFont.LEFT);
                Skill firstLevel = template.skills != null && template.skills.Length > 0 ? template.skills[0] : null;
                string requirement = firstLevel != null ? "Cần " + NinjaUtil.getMoneys(firstLevel.powRequire) + " tiềm năng để học" : "Chưa học";
                mFont.tahoma_7_grey.drawString(g, TruncateString(mFont.tahoma_7_grey, requirement, _leftColRect.Width - 46), textX, y + 19, mFont.LEFT);
            }
        }

        private void PaintSkillDetailColumn(mGraphics g)
        {
            g.setColor(0xEEE7DC);
            g.fillRect(_rightBodyRect.X, _rightBodyRect.Y, _rightBodyRect.Width, _rightBodyRect.Height);
            g.setColor(0xC4B79B);
            g.drawRect(_rightBodyRect.X, _rightBodyRect.Y, _rightBodyRect.Width, _rightBodyRect.Height);
            if (_showIntrinsicList)
            {
                PaintIntrinsicList(g);
                return;
            }
            if (_selectedSkillRow < 0) return;
            if (_selectedSkillRow < PotentialStatRowCount) PaintPotentialDetail(g);
            else if (_selectedSkillRow == IntrinsicRowIndex) PaintIntrinsicDetail(g);
            else PaintTemplateSkillDetail(g);
        }

        private void PaintPotentialDetail(mGraphics g)
        {
            Char me = Char.myCharz();
            if (me == null) return;
            long increase = GetPotentialIncreaseValue(me, _selectedSkillRow);
            int action = IsPotentialActionVisible(_selectedPotentialAction) ? _selectedPotentialAction : GetFirstVisiblePotentialAction();
            string suffix = _selectedSkillRow == 4 ? "%" : string.Empty;
            string detail;
            if (action < 0)
            {
                detail = "Chưa đủ tiềm năng để tăng " + PotentialNames[_selectedSkillRow] + ".";
            }
            else if (action == 3)
            {
                detail = "Tự động tăng " + PotentialNames[_selectedSkillRow] + " theo số lượng đã nhập.";
            }
            else
            {
                int batch = GetPotentialBatch(action);
                detail = "Sử dụng " + NinjaUtil.getMoneys(GetPotentialBatchCost(me, _selectedSkillRow, batch))
                    + " tiềm năng để nâng " + increase * batch + suffix + " " + PotentialNames[_selectedSkillRow] + ".";
            }
            DrawWrappedText(g, mFont.tahoma_7b_dark, detail, _rightBodyRect.X + 12, _rightBodyRect.Y + 8, _rightBodyRect.Width - 24);

            for (int i = 0; i < _potentialButtonRects.Length; i++)
            {
                if (!IsPotentialActionVisible(i)) continue;
                string label = i == 3 ? "Tăng\ntự động" : "Tăng " + increase * GetPotentialBatch(i) + suffix;
                PaintSkillActionButton(g, _potentialButtonRects[i], label, _skillFocusArea == SkillFocusDetail && i == action);
            }
        }

        private void PaintIntrinsicDetail(mGraphics g)
        {
            EnsureDefaultIntrinsicActions();
            int centerX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            mFont.tahoma_7b_dark.drawString(g, "Nội tại", centerX, _rightBodyRect.Y + 8, mFont.CENTER);
            if (Panel.specialInfo != null && Panel.spearcialImage >= 0)
                SmallImage.drawSmallImage(g, Panel.spearcialImage, centerX, _rightBodyRect.Y + 35, 0, 3);
            string detail = !string.IsNullOrEmpty(Panel.specialInfo)
                ? Panel.specialInfo
                : (!string.IsNullOrEmpty(_intrinsicDialogText) ? _intrinsicDialogText : "Chưa mở nội tại.");
            DrawWrappedText(g, mFont.tahoma_7_green2, detail, _rightBodyRect.X + 12, _rightBodyRect.Y + 58, _rightBodyRect.Width - 24);

            if (_intrinsicActionCount <= 0) return;
            int dividerY = GetIntrinsicActionRect(0).Y - 7;
            g.setColor(0xB29468);
            g.drawLine(_rightBodyRect.X + 10, dividerY, _rightBodyRect.X + _rightBodyRect.Width - 10, dividerY);
            for (int i = 0; i < _intrinsicActionCount; i++)
            {
                bool focused = _skillFocusArea == SkillFocusDetail && i == _selectedIntrinsicAction;
                PaintSkillActionButton(g, GetIntrinsicActionRect(i), _intrinsicActionLabels[i], focused);
            }
        }

        private void PaintIntrinsicList(mGraphics g)
        {
            Char me = Char.myCharz();
            int count = GetIntrinsicListCount();
            if (me == null || count <= 0)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa có danh sách nội tại.",
                    _rightBodyRect.X + _rightBodyRect.Width / 2, _rightBodyRect.Y + 12, mFont.CENTER);
                return;
            }

            using (UiRenderState.Push(g, _rightBodyRect, clip: true))
            {
                int scrollY = _rightScrollAdapter != null ? _rightScrollAdapter.ScrollY : 0;
                for (int i = 0; i < count; i++)
                {
                    int rowY = _rightBodyRect.Y + i * IntrinsicListRowHeight - scrollY;
                    if (rowY + IntrinsicListRowHeight <= _rightBodyRect.Y
                        || rowY >= _rightBodyRect.Y + _rightBodyRect.Height) continue;
                    bool selected = i == _selectedIntrinsicListIndex;
                    g.setColor(selected ? 0xFFD038 : 0xF2F0ED);
                    g.fillRect(_rightBodyRect.X + 1, rowY, _rightBodyRect.Width - 2, IntrinsicListRowHeight - 1);
                    g.setColor(0xC4B79B);
                    g.drawLine(_rightBodyRect.X + 1, rowY + IntrinsicListRowHeight - 1,
                        _rightBodyRect.X + _rightBodyRect.Width - 2, rowY + IntrinsicListRowHeight - 1);

                    PaintIntrinsicIconFrame(g, _rightBodyRect.X + 3, rowY + 2, selected);
                    if (me.imgSpeacialSkill != null && me.imgSpeacialSkill.Length > 0
                        && me.imgSpeacialSkill[0] != null && i < me.imgSpeacialSkill[0].Length)
                        SmallImage.drawSmallImage(g, me.imgSpeacialSkill[0][i], _rightBodyRect.X + 18,
                            rowY + IntrinsicListRowHeight / 2, 0, 3);

                    string info = me.infoSpeacialSkill[0][i] ?? string.Empty;
                    string[] lines = mFont.tahoma_7_grey.splitFontArray(info, _rightBodyRect.Width - 46);
                    int textX = _rightBodyRect.X + 36;
                    if (lines.Length > 0)
                        mFont.tahoma_7_blue.drawString(g, lines[0], textX, rowY + 4, mFont.LEFT);
                    if (lines.Length > 1)
                        mFont.tahoma_7_grey.drawString(g, lines[1], textX, rowY + 19, mFont.LEFT);
                }
            }
        }

        private static void PaintIntrinsicIconFrame(mGraphics g, int x, int y, bool selected)
        {
            g.setColor(selected ? 0x8B42F4 : 0xE68A00);
            g.fillRect(x, y, 31, 31);
            g.setColor(0xFFD15A);
            g.fillRect(x + 2, y + 2, 27, 27);
            g.setColor(0xE7F4F4);
            g.fillRect(x + 4, y + 4, 23, 23);
            g.setColor(0x9B6500);
            g.drawRect(x, y, 30, 30);
        }

        private void PaintIntrinsicSideButtons(mGraphics g)
        {
            bool backFocused = _skillFocusArea == SkillFocusIntrinsicSide && _selectedIntrinsicSideAction == 0;
            PaintSkillActionButton(g, _intrinsicSideButtonRects[0], "Quay lại", backFocused);
            if (!_showIntrinsicList || _selectedIntrinsicListIndex < 0) return;
            bool selectFocused = _skillFocusArea == SkillFocusIntrinsicSide && _selectedIntrinsicSideAction == 1;
            PaintSkillActionButton(g, _intrinsicSideButtonRects[1], "Chọn chỉ số", selectFocused);
        }

        private void PaintIntrinsicInput(mGraphics g)
        {
            g.setColor(0xD7C4A9);
            g.fillRect(_intrinsicInputDialogRect.X, _intrinsicInputDialogRect.Y,
                _intrinsicInputDialogRect.Width, _intrinsicInputDialogRect.Height);
            g.setColor(0x6B5745);
            g.drawRect(_intrinsicInputDialogRect.X, _intrinsicInputDialogRect.Y,
                _intrinsicInputDialogRect.Width, _intrinsicInputDialogRect.Height);
            mFont.tahoma_7b_dark.drawString(g, "Nhập chỉ số", _intrinsicInputDialogRect.X + 14,
                _intrinsicInputDialogRect.Y + 7, mFont.LEFT);

            g.setColor(0xF7C400);
            g.fillRect(_intrinsicInputCloseRect.X, _intrinsicInputCloseRect.Y,
                _intrinsicInputCloseRect.Width, _intrinsicInputCloseRect.Height);
            g.setColor(0xE59600);
            g.drawRect(_intrinsicInputCloseRect.X, _intrinsicInputCloseRect.Y,
                _intrinsicInputCloseRect.Width, _intrinsicInputCloseRect.Height);
            g.setColor(0xE35A24);
            g.drawLine(_intrinsicInputCloseRect.X + 5, _intrinsicInputCloseRect.Y + 5,
                _intrinsicInputCloseRect.X + 15, _intrinsicInputCloseRect.Y + 15);
            g.drawLine(_intrinsicInputCloseRect.X + 15, _intrinsicInputCloseRect.Y + 5,
                _intrinsicInputCloseRect.X + 5, _intrinsicInputCloseRect.Y + 15);

            _intrinsicInputField?.paint(g);
            g.setClip(0, 0, GameCanvas.w, GameCanvas.h + 1);
            PaintSkillActionButton(g, _intrinsicNormalButtonRect, "Mở thường", _intrinsicInputFocus == 1);
            PaintSkillActionButton(g, _intrinsicVipButtonRect, "Mở VIP", _intrinsicInputFocus == 2);
        }

        private void PaintTemplateSkillDetail(mGraphics g)
        {
            SkillTemplate template = GetSelectedSkillTemplate();
            if (template == null) return;
            Skill skill = GetSelectedSkill();
            int centerX = _rightBodyRect.X + _rightBodyRect.Width / 2;
            int y = _rightBodyRect.Y + 7;
            mFont.tahoma_7b_dark.drawString(g, template.name, centerX, y, mFont.CENTER);
            y += 15;

            if (template.description != null)
            {
                for (int i = 0; i < template.description.Length; i++)
                {
                    string[] lines = mFont.tahoma_7_green2.splitFontArray(template.description[i] ?? string.Empty, _rightBodyRect.Width - 24);
                    for (int j = 0; j < lines.Length; j++)
                    {
                        mFont.tahoma_7_green2.drawString(g, lines[j], centerX, y, mFont.CENTER);
                        y += 13;
                    }
                }
            }

            y += 4;
            g.setColor(0xB29468);
            g.drawLine(_rightBodyRect.X + 10, y, _rightBodyRect.X + _rightBodyRect.Width - 10, y);
            y += 8;

            if (skill == null)
            {
                mFont.tahoma_7_grey.drawString(g, "Chưa học", centerX, y, mFont.CENTER);
                Skill firstLevel = template.skills != null && template.skills.Length > 0 ? template.skills[0] : null;
                if (firstLevel != null)
                    mFont.tahoma_7_blue.drawString(g, "Cần " + NinjaUtil.getMoneys(firstLevel.powRequire) + " tiềm năng", centerX, y + 15, mFont.CENTER);
                return;
            }

            mFont.tahoma_7_blue.drawString(g, "Cấp độ: " + skill.getDisplayLevel(), centerX, y, mFont.CENTER);
            y += 14;
            string damageInfo = NinjaUtil.Replace(template.damInfo ?? string.Empty, "#", skill.damage + string.Empty);
            mFont.tahoma_7_blue.drawString(g, damageInfo, centerX, y, mFont.CENTER);
            y += 14;
            mFont.tahoma_7_blue.drawString(g, "KI tiêu hao: " + skill.manaUse + (template.manaUseType == 1 ? "%" : string.Empty), centerX, y, mFont.CENTER);
            y += 14;
            mFont.tahoma_7_blue.drawString(g, "Hồi chiêu: " + skill.strTimeReplay() + "s", centerX, y, mFont.CENTER);

            int dividerY = _assignSkillButtonRect.Y - 7;
            g.setColor(0xB29468);
            g.drawLine(_rightBodyRect.X + 10, dividerY, _rightBodyRect.X + _rightBodyRect.Width - 10, dividerY);
            PaintSkillActionButton(g, _assignSkillButtonRect, "Gán phím ô", _showSkillKeyPicker || _skillFocusArea == SkillFocusDetail);
        }

        private static void PaintSkillActionButton(mGraphics g, UiRect rect, string label, bool active)
        {
            g.setColor(active ? 0x55BE00 : 0xE99A00);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(active ? 0x438F00 : 0xB86F00);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);
            int newline = label.IndexOf('\n');
            if (newline < 0)
            {
                mFont.tahoma_7b_dark.drawString(g, label, rect.X + rect.Width / 2, rect.Y + (rect.Height - 9) / 2, mFont.CENTER);
                return;
            }
            mFont.tahoma_7b_dark.drawString(g, label.Substring(0, newline), rect.X + rect.Width / 2, rect.Y + 3, mFont.CENTER);
            mFont.tahoma_7b_dark.drawString(g, label.Substring(newline + 1), rect.X + rect.Width / 2, rect.Y + 15, mFont.CENTER);
        }

        private void PaintSkillKeyPicker(mGraphics g)
        {
            Skill selectedSkill = GetSelectedSkill();
            if (selectedSkill == null) return;
            bool useTouchSlots = GameCanvas.isTouch && !Main.isPC;
            Skill[] slots = useTouchSlots ? GameScr.onScreenSkill : GameScr.keySkill;
            for (int i = 0; i < _skillKeyButtonRects.Length; i++)
            {
                bool assigned = slots != null && i < slots.Length && slots[i] != null && slots[i].template != null
                    && slots[i].template.id == selectedSkill.template.id;
                bool focused = _skillFocusArea == SkillFocusKeys && i == _selectedSkillKeyIndex;
                PaintSkillActionButton(g, _skillKeyButtonRects[i], "Phím " + (i + 1), assigned || focused);
                if (focused)
                {
                    g.setColor(0xFFFFFF);
                    g.drawRect(_skillKeyButtonRects[i].X + 2, _skillKeyButtonRects[i].Y + 2,
                        _skillKeyButtonRects[i].Width - 4, _skillKeyButtonRects[i].Height - 4);
                }
            }
        }

        private void PaintTaskTabContent(mGraphics g)
        {
            PaintSubTabs(g);
            PaintTaskListColumn(g);
            PaintTaskDetailColumn(g);
        }

        private void PaintSubTabs(mGraphics g)
        {
            // SubTab 0: Nhiệm vụ chính
            bool isTab0 = (_selectedSubTab == 0);
            g.setColor(isTab0 ? 0x63BE00 : 0xE99A00);
            g.fillRect(_subTab0Rect.X, _subTab0Rect.Y, _subTab0Rect.Width, _subTab0Rect.Height);
            g.setColor(isTab0 ? 0x527D00 : 0xA56800);
            g.drawRect(_subTab0Rect.X, _subTab0Rect.Y, _subTab0Rect.Width, _subTab0Rect.Height);
            mFont.tahoma_7b_dark.drawString(g, "Nhiệm vụ chính", _subTab0Rect.X + _subTab0Rect.Width / 2, _subTab0Rect.Y + 4, mFont.CENTER);

            // SubTab 1: Nhiệm vụ khác
            bool isTab1 = (_selectedSubTab == 1);
            g.setColor(isTab1 ? 0x63BE00 : 0xE99A00);
            g.fillRect(_subTab1Rect.X, _subTab1Rect.Y, _subTab1Rect.Width, _subTab1Rect.Height);
            g.setColor(isTab1 ? 0x527D00 : 0xA56800);
            g.drawRect(_subTab1Rect.X, _subTab1Rect.Y, _subTab1Rect.Width, _subTab1Rect.Height);
            mFont.tahoma_7b_dark.drawString(g, "Nhiệm vụ khác", _subTab1Rect.X + _subTab1Rect.Width / 2, _subTab1Rect.Y + 4, mFont.CENTER);
        }

        private void PaintTaskListColumn(mGraphics g)
        {
            UiRect listViewport = GetLeftListViewport();
            g.setColor(0xC4B79B);
            g.drawRect(listViewport.X, listViewport.Y, listViewport.Width, listViewport.Height);

            using (UiRenderState.Push(g, listViewport, clip: true))
            {
                g.setColor(0xDFD2BC);
                g.fillRect(_leftColRect.X, _leftColRect.Y, _leftColRect.Width, _leftColRect.Height);

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

                        // Row background
                        g.setColor(isSelected ? 0xFFD038 : (status == 2 ? 0xD8D8D8 : 0xF2F0ED));
                        g.fillRect(_leftColRect.X + 1, rowY, _leftColRect.Width - 2, MainTaskRowHeight - 1);

                        g.setColor(status == 2 ? 0xC4B9A8 : 0xB8A58B);
                        g.fillRect(_leftColRect.X + 1, rowY, numberColW, MainTaskRowHeight - 1);
                        mFont.tahoma_7b_dark.drawString(g, (i + 1).ToString(), _leftColRect.X + numberColW / 2, rowY + 8, mFont.CENTER);

                        // Divider line
                        g.setColor(0xC4B79B);
                        int divY = rowY + MainTaskRowHeight - 1;
                        if (divY >= clipTop && divY < clipBottom)
                        {
                            g.drawLine(_leftColRect.X + 1, divY, _leftColRect.X + _leftColRect.Width - 2, divY);
                        }

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
            g.setColor(selected ? 0xFFD038 : 0xF2F0ED);
            g.fillRect(rect.X, rect.Y, rect.Width, rect.Height);
            g.setColor(0xC7B79F);
            g.drawRect(rect.X, rect.Y, rect.Width, rect.Height);

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
            g.setColor(0xDFCFB7);
            g.fillRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);
            g.setColor(0xB89261);
            g.drawRect(_rightColRect.X, _rightColRect.Y, _rightColRect.Width, _rightColRect.Height);

            g.setColor(0xEEE7DC);
            g.fillRect(_rightColRect.X + 2, _rightColRect.Y + 1, _rightColRect.Width - 4, 21);
            g.setColor(0xB8A58B);
            g.drawRect(_rightColRect.X + 2, _rightColRect.Y + 1, _rightColRect.Width - 4, 21);
            mFont.tahoma_7_orange.drawString(g, "Chi tiết nhiệm vụ", _rightColRect.X + _rightColRect.Width / 2, _rightColRect.Y + 5, mFont.CENTER);

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

        private int DrawWrappedText(mGraphics g, mFont font, string text, int x, int y, int width)
        {
            string[] lines = font.splitFontArray(text ?? string.Empty, width);
            for (int i = 0; i < lines.Length; i++)
            {
                font.drawString(g, lines[i], x, y, mFont.LEFT);
                y += 14;
            }
            return y;
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

        private string TruncateString(mFont font, string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (font.getWidth(text) <= maxWidth) return text;
            string s = text;
            while (s.Length > 0 && font.getWidth(s + "...") > maxWidth)
            {
                s = s.Substring(0, s.Length - 1);
            }
            return s + "...";
        }

        // Icons
        private void DrawBullIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x8D6E63);
            g.fillRect(x + 4, y + 6, 12, 10);
            g.setColor(0xD7CCC8);
            g.fillRect(x + 2, y + 2, 4, 5);
            g.fillRect(x + 14, y + 2, 4, 5);
            g.setColor(0xBCAAA4);
            g.fillRect(x + 6, y + 11, 8, 5);
            g.setColor(0x3E2723);
            g.fillRect(x + 6, y + 8, 2, 2);
            g.fillRect(x + 12, y + 8, 2, 2);
        }

        private void DrawKanaoIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x9B59B6);
            g.fillRect(x + 3, y + 4, 6, 6);
            g.fillRect(x + 11, y + 4, 6, 6);
            g.fillRect(x + 4, y + 10, 5, 5);
            g.fillRect(x + 11, y + 10, 5, 5);
            g.setColor(0x2C3E50);
            g.fillRect(x + 9, y + 3, 2, 12);
        }

        private void DrawFishIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x3498DB);
            g.fillRect(x + 4, y + 6, 10, 6);
            g.setColor(0x2980B9);
            g.fillRect(x + 2, y + 7, 2, 4);
            g.fillRect(x + 14, y + 4, 3, 10);
            g.setColor(0xFFFFFF);
            g.fillRect(x + 5, y + 7, 2, 2);
        }

        private void DrawCheckmark(mGraphics g, int x, int y, int size)
        {
            g.setColor(0x27AE60);
            int cx = x + 3;
            int cy = y + size / 2;
            for (int i = 0; i < 3; i++)
            {
                g.drawLine(cx + i, cy + i, cx + i + 1, cy + i + 1);
            }
            for (int i = 0; i < 7; i++)
            {
                g.drawLine(cx + 3 + i, cy + 3 - i, cx + 4 + i, cy + 3 - i);
            }
        }

        private void DrawLock(mGraphics g, int x, int y, int w, int h)
        {
            g.setColor(0x7F8C8D);
            g.drawRect(x + 2, y + 1, w - 4, h / 2);
            g.fillRect(x, y + h / 2, w, h / 2);
            g.setColor(0x2C3E50);
            g.fillRect(x + w / 2 - 1, y + h / 2 + 2, 2, 3);
        }

        private void DrawBulletPoint(mGraphics g, int x, int y, int color)
        {
            g.setColor(color);
            g.fillRect(x, y, 3, 3);
        }

        private void DrawScrollIcon(mGraphics g, int x, int y)
        {
            g.setColor(0xF9E79F);
            g.fillRect(x + 5, y + 3, 16, 18);
            g.setColor(0xD4AC0D);
            g.drawRect(x + 5, y + 3, 16, 18);
            g.setColor(0xB7950B);
            g.fillRect(x + 3, y + 2, 4, 20);
            g.fillRect(x + 19, y + 2, 4, 20);
            g.setColor(0x7D6608);
            g.drawLine(x + 8, y + 8, x + 18, y + 8);
            g.drawLine(x + 8, y + 12, x + 18, y + 12);
            g.drawLine(x + 8, y + 16, x + 15, y + 16);
        }

        private void DrawBackpackIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x8B4513);
            g.fillRect(x + 4, y + 6, 18, 15);
            g.setColor(0x5C2D0C);
            g.drawRect(x + 4, y + 6, 18, 15);
            g.setColor(0xA0522D);
            g.fillRect(x + 4, y + 4, 18, 6);
            g.setColor(0xF1C40F);
            g.fillRect(x + 11, y + 9, 4, 3);
            g.setColor(0x5C2D0C);
            g.drawLine(x + 9, y + 3, x + 17, y + 3);
            g.drawLine(x + 9, y + 3, x + 9, y + 5);
            g.drawLine(x + 17, y + 3, x + 17, y + 5);
        }

        private void DrawBookIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x2471A3);
            g.fillRect(x + 5, y + 3, 16, 18);
            g.setColor(0x1B4F72);
            g.drawRect(x + 5, y + 3, 16, 18);
            g.setColor(0x5499C7);
            g.fillRect(x + 3, y + 3, 4, 18);
            g.setColor(0xF1C40F);
            g.fillRect(x + 10, y + 8, 7, 7);
            g.setColor(0x2471A3);
            g.fillRect(x + 12, y + 10, 3, 3);
        }

        private void DrawShieldIcon(mGraphics g, int x, int y)
        {
            g.setColor(0xC0392B);
            g.fillRect(x + 5, y + 4, 16, 12);
            for (int i = 0; i < 6; i++)
            {
                g.drawLine(x + 5 + i, y + 16 + i, x + 20 - i, y + 16 + i);
            }
            g.setColor(0x78281F);
            g.drawRect(x + 5, y + 4, 16, 12);
            g.setColor(0xF1C40F);
            g.fillRect(x + 10, y + 8, 6, 6);
        }

        private void DrawGearIcon(mGraphics g, int x, int y)
        {
            g.setColor(0x7F8C8D);
            g.fillRect(x + 7, y + 7, 12, 12);
            g.setColor(0x34495E);
            g.drawRect(x + 7, y + 7, 12, 12);
            g.setColor(0xD5C7B0);
            g.fillRect(x + 11, y + 11, 4, 4);
            g.setColor(0x7F8C8D);
            g.fillRect(x + 11, y + 4, 4, 3);
            g.fillRect(x + 11, y + 19, 4, 3);
            g.fillRect(x + 4, y + 11, 3, 4);
            g.fillRect(x + 19, y + 11, 3, 4);
        }
    }
}

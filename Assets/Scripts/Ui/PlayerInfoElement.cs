using TMPro;
using UnityEngine;

namespace Ui
{
    public class PlayerInfoElement : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _rankText;
        [SerializeField] private TextMeshPro _nickNameText;
        [SerializeField] private TextMeshPro _scoreText;
        [SerializeField] private CustomImage _background;
        private PlayerData _assignedData;
        public PlayerData AssignedData => _assignedData;
        public int Rank { get; private set; }
        public void PopulateView(PlayerData data, int rank)
        {
            Rank = rank;
            _assignedData = new PlayerData(data.Id, data.Nickname, data.Score);
            _rankText.text = rank.ToString();
            _nickNameText.text = data.Nickname;
            _scoreText.text = data.Score.ToString();

            if (data.Id == 0)
            {
                SetBackgroundColor(Color.green);
                _background.GetComponent<SpriteRenderer>().sortingOrder = 50;
                _rankText.sortingOrder = 50;
                _nickNameText.sortingOrder = 50;
                _scoreText.sortingOrder = 50;
            }
            else
            {
                SetBackgroundColor(Color.white);
            }
        }

        public void SetBackgroundColor(Color color)
        {
            _background.SetColor(color);
        }
    }
}
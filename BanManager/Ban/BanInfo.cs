using Newtonsoft.Json;
using RecentlyUserQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BanManagerPlugin.Ban
{
    public enum BanAccessType
    {
        NotBanned,//没被ban的可以发言给osu!irc
        Whitelist,//只有白名单的人可以发言
        All//全都可以发言
    }

    public class BanInfo
    {
        public class Rule
        {
            public int RuleID { get; set; }
            public string RuleExpression { get; set; }

            [JsonIgnore]
            public Regex regex { get; set; }
        }

        public List<Rule> BanRules { get; set; } = new List<Rule>();

        public List<string> BanUsers {
            get;
            set; } = new List<string>();

        public List<string> WhitelistUsers { get; set; } = new List<string>();

        public List<Rule> WhitelistRules { get; set; } = new List<Rule>();

        [JsonIgnore]
        internal int rule_id_gerenator;

        public BanAccessType AccessType { get; set; } = BanAccessType.NotBanned;

        private int GenRuleId()
        {
            return rule_id_gerenator;
        }

        /// <summary>
        /// 添加用户名到禁烟黑名单
        /// </summary>
        /// <param name="userName">用户名</param>
        public void AddBanUserName(string userName)
        {
            RemoveWhiteListUserName(userName);
            if (!BanUsers.Contains(userName))
                BanUsers.Add(userName);
        }

        private static Rule CreateRule(string expr, int id) => new Rule()
        {
            RuleExpression = expr,
            RuleID = id,
            regex = new Regex(expr)
        };

        /// <summary>
        /// 添加禁言规则，符合正则表达式匹配的用户名都被加入黑名单
        /// </summary>
        /// <param name="ruleRegexExpr">正则表达式</param>
        /// <returns></returns>
        public int AddBanRuleRegex(string ruleRegexExpr)
        {
            Rule rule = CreateRule(ruleRegexExpr, GenRuleId());
            
            return rule.RuleID;
        }

        private int AddBanRuleRegex(string ruleRegexExpr, int id)
        {
            BanRules.RemoveAll(r => r.RuleID == id);

            BanRules.Add(CreateRule(ruleRegexExpr, GenRuleId()));
            return id;
        }

        /// <summary>
        /// 添加用户名到白名单
        /// </summary>
        /// <param name="userName">用户名</param>
        public void AddWhiteListUserName(string userName)
        {
            RemoveBanUserName(userName);
            if (!WhitelistUsers.Contains(userName))
                WhitelistUsers.Add(userName);
        }

        /// <summary>
        /// 将某个用户从黑名单中移除
        /// </summary>
        /// <param name="userName">用户名</param>
        public void RemoveBanUserName(string userName)
        {
            if (BanUsers.Contains(userName))
                BanUsers.Remove(userName);
        }

        /// <summary>
        /// 将某个用户从白名单中移除
        /// </summary>
        /// <param name="userName">用户名</param>
        public void RemoveWhiteListUserName(string userName)
        {
            if (WhitelistUsers.Contains(userName))
                WhitelistUsers.Remove(userName);
        }

        /// <summary>
        /// 添加白名单规则，符合正则表达式匹配的用户名都被加入白名单
        /// </summary>
        /// <param name="ruleRegexExpr">正则表达式</param>
        /// <returns></returns>
        public int AddWhiteListRuleRegex(string ruleRegexExpr)
        {
            Rule rule = CreateRule(ruleRegexExpr, GenRuleId());
            WhitelistRules.Add(rule);
            return rule.RuleID;
        }

        public void RemoveWhiteListRuleRegex(int ruleId)
        {
            for(int i = 0; i < WhitelistRules.Count; i++)
            {
                if (WhitelistRules[i].RuleID == ruleId)
                {
                    WhitelistRules.RemoveAt(i);
                    break;
                }
                    
            }
        }

        public void RemovBanListRuleRegex(int ruleId)
        {
            for (int i = 0; i < BanRules.Count; i++)
            {
                if (BanRules[i].RuleID == ruleId)
                {
                    BanRules.RemoveAt(i);
                    break;
                }

            }
        }

        private int AddWhiteListRuleRegex(string ruleRegexExpr, int id)
        {
            WhitelistRules.Add(CreateRule(ruleRegexExpr, id));
            return id;
        }

        public void AddWhiteListId(int id)
        {
            string userName = GetUserName(id);
            if (userName.Length != 0)
                AddWhiteListUserName(userName);
        }

        public void RemoveWhiteListId(int id)
        {
            string userName = GetUserName(id);
            if (userName.Length != 0)
                RemoveWhiteListUserName(userName);
        }

        public void AddBanId(int id)
        {
            string userName = GetUserName(id);
            if (userName.Length != 0)
                AddBanUserName(userName);
        }

        public void RemoveBanId(int id)
        {
            string userName = GetUserName(id);
            if (userName.Length != 0)
                RemoveBanUserName(userName);
        }

        /// <summary>
        /// 将当前过滤控制器的内容格式化打包，获得的字符串可以通过LoadFromFormattedString()回复 
        /// </summary>
        /// <returns></returns>
        public string SaveAsFormattedString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// 按照字符串内容加载内容,顺带清除重复冲突的内容
        /// </summary>
        /// <param name="formatString">字符串内容，通常由SaveAsFormattedString()得到的</param>
        public static BanInfo LoadFromJSON(string formatString)
        {
            BanInfo info = JsonConvert.DeserializeObject<BanInfo>(formatString);

            Action<Rule> gen_regex = r => r.regex = new Regex(r.RuleExpression);

            info.BanRules.ForEach(gen_regex);
            info.WhitelistRules.ForEach(gen_regex);

            var list = info.WhitelistRules.Concat(info.BanRules);

            info.rule_id_gerenator = list.Count()!=0?list.Max(r=>r.RuleID):0;

            HashSet<int> check = new HashSet<int>();
            Predicate<Rule> check_func = p =>
            {
                if (check.Contains(p.RuleID))
                    return true;
                check.Add(p.RuleID);
                return false;
            };

            info.WhitelistRules.RemoveAll(check_func);
            info.BanRules.RemoveAll(check_func);

            HashSet<string> check_str = new HashSet<string>();
            Predicate<string> check_str_func = p =>
            {
                if (check_str.Contains(p))
                    return true;
                check_str.Add(p);
                return false;
            };

            info.WhitelistUsers.RemoveAll(check_str_func);
            check_str.Clear();
            info.BanUsers.RemoveAll(check_str_func);

            return info;
        }

        /// <summary>
        /// 判断用户是否被ban
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <returns></returns>
        public bool IsBanned(string userName)
        {
            if (AccessType == BanAccessType.All)
                return false;
            if (AccessType == BanAccessType.Whitelist)
            {
                foreach (var enumUserName in WhitelistUsers)
                    if (enumUserName.CompareTo(userName) == 0)
                        return false;
                foreach (var enumRule in WhitelistRules)
                    if (enumRule.regex.IsMatch(userName))
                        return false;
                return true;
            }
            if (AccessType == BanAccessType.NotBanned)
            {
                foreach (var enumUserName in BanUsers)
                    if (enumUserName.CompareTo(userName) == 0)
                        return true;
                foreach (var enumRule in BanRules)
                    if (enumRule.regex.IsMatch(userName))
                        return true;
                return false;
            }
            return false;
        }

        public bool IsAllow(string userName)
        {
            return !IsBanned(userName);
        }

        private string GetUserName(int id)
        {
            return UserIdGenerator.GetUserName(id);
        }
    }
}

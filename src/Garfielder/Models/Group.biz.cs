﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Garfielder.Core.Infrastructure;
using Garfielder.ViewModels;
using System.Data;
using System.Data.Objects;

namespace Garfielder.Models
{
    /// <summary>
    /// biz logic for Group.TODO:via repository pattern
    /// </summary>
    public partial class Group
    {
        public int CntTopic { get; set; }
        /// <summary>
        /// validate name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ValidateName(string name, string id = "00000000-0000-0000-0000-000000000000")
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            var id1 = Guid.Parse(id);
            var valid = false;
            if(id1.Equals(Guid.Empty))
            {
                valid = ListAll().Count(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) == 0;
            }else
            {
                valid = ListAll().Count(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)&&x.Id!=id1) == 0;
            }
            
            return valid;
        }
        /// <summary>
        /// validate slug
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool ValidateSlug(string slug, string id = "00000000-0000-0000-0000-000000000000")
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            var id1 = Guid.Parse(id);
            var valid = false;
            if (id1.Equals(Guid.Empty))
            {
                valid = ListAll().Count(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)) == 0;
            }else
            {
                valid = ListAll().Count(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase)&&x.Id!=id1) == 0;
            }
            return valid;
        }

        /// <summary>
        /// list all group data.TODO:Don't use Group!You should define an interface type such as IGroupData
        /// </summary>
        /// <returns></returns>
        public static List<Group> ListAll() {
            using (var db = new GarfielderEntities())
            {
                if ( _Groups== null)
				{
					lock (_SyncRoot)
					{
                        if (_Groups == null)
						{
                            _Groups = db.Groups.ToList();
                            _Groups.ForEach(x => x.CntTopic = x.Topics.Count);
						}
					}
				}

                return _Groups;
			}//using
            
        }
        /// <summary>
        /// list all group data.TODO:Don't use VMGroupEdit!You should define an interface type such as IGroupData
        /// </summary>
        /// <returns></returns>
        public static List<VMGroupEdit> ListAllData()
        {
            var items = ListAll().OrderByDescending(x=>x.CreatedAt).ToList();
            var r = new List<VMGroupEdit>();
            items.ForEach(x => r.Add(
                    new VMGroupEdit
                    {
                        Name = x.Name,
                        Slug = x.Slug,
                        Id = x.Id,
                        Level = x.Level,
                        Description = x.Description,
                        ParentID = x.ParentID,
                        ParentName = x.Parent == null ? "" : x.Parent.Name,
                        CntTopic = x.Topics.Count,
                        Sys=x.Sys
                    }
                )
            );
            return r;

        }
        /// <summary>
        /// clear cache
        /// </summary>
        public static void ClearCache()
        {
            _Groups = null;
        }

        /// <summary>
        /// get by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dbToAttach"></param>
        /// <returns></returns>
        public static Group Get(Guid id,GarfielderEntities dbToAttach=null)
        {
            var obj=ListAll().SingleOrDefault(x => x.Id == id);
            if(dbToAttach!=null&&null!=obj)
            {
                dbToAttach.Groups.Attach(obj);
            }
                
            return obj;
        }
        /// <summary>
        /// get full group data
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static VMGroupHome Get(string slug)
        {
            var retVal = new VMGroupHome();
            try
            {
                using (var db=new GarfielderEntities())
                {
                    var g = db.Groups.SingleOrDefault(x=>x.Slug.Equals(slug));
                    if(g==null)
                    {
                        return retVal;
                    }

                    retVal.Topics=new List<VMTopic>();
                    g.Topics.ToList().ForEach(x=>
                                                  {
													  var tempFile = db.XFiles.SingleOrDefault(y => y.Meta.IndexOf(x.Logo) >= 0) ??
			   x.XFiles.OrderByDescending(y => y.Title).FirstOrDefault();
                                                      if (null != tempFile)
                                                      {
                                                          x.Icon = new VMXFileEdit
                                                          {
                                                              Id = tempFile.Id,
                                                              Title = tempFile.Title,
                                                              Extension = tempFile.Extension,
                                                              Description = tempFile.Description,
                                                              UserName = tempFile.User.Name,
                                                              Name = tempFile.Name,
                                                              CreatedAt = tempFile.CreatedAt
                                                          };
                                                      }
                                                      
                                                      retVal.Topics.Add(new VMTopic
                                                                            {
                                                                                Id = x.Id,
                                                                                Title = x.Title,
                                                                                Slug = x.Slug,
                                                                                Desc = x.Description,
                                                                                Logo =
                                                                                    x.Icon == null
                                                                                        ? string.Format(
                                                                                            "{0}assets/img/default.jpg",
                                                                                            Web.Utils.AbsoluteWebRoot)
                                                                                        : x.Icon.Url(ImageFlags.S160X100),
                                                                                DateCreated = x.CreatedAt

                                                                            });
                                                  });

                    retVal.GroupData = new VMGroupEdit()
                                           {
                                               Name = g.Name,
                                               Slug = g.Slug,
                                               Id = g.Id,
                                               Level = g.Level,
                                               Description = g.Description,
                                               ParentID = g.ParentID,
                                               ParentName = g.Parent == null ? "" : g.Parent.Name,
                                               CntTopic = g.Topics.Count,
                                               Sys = g.Sys
                                           };

                    retVal.TopicNum = g.Topics.Count;

                }//using
            }
            catch (Exception ex)
            {
                //TODO:log
                
            }
            return retVal;
        }
        /// <summary>
        /// delete by specified id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Msg DeleteByID(params Guid[] id)
        {
            var r = new Msg();
            if (id == null || id.Length == 0)
            {
                r.Error = true;
                r.Body = "No selected topics to be deleted!";
                return r;
            }
            using (var db = new GarfielderEntities())
            {
                try
                {
                    var items = db.Groups.Where(x => id.Contains(x.Id)).ToList();
                    items.ForEach(obj =>
                    {
                        obj.Topics.Clear();
                        db.Groups.DeleteObject(obj);
                    });
                    db.SaveChanges();
                    ClearCache();
                }
                catch (Exception ex)
                {
                    r.Error = true;
                    r.Body = ex.Message;
                }
            }//using
            return r;
        }
        /// <summary>
        /// check the specified slug whether exists.if it exists we provide a new one 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public static string CheckSlug(GarfielderEntities db, string slug)
        {
            var tag = db.Groups.SingleOrDefault(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            if (null == tag)
            {
                return slug;
            }
            return string.Format("{0}{1}", slug, Utils.RandomStr(5));
        }
        /// <summary>
        /// save a item.update if exists
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static dynamic Save(VMGroupEdit obj) {
            var msg = new Msg();
            var dbm = default(Group);
            obj.Slug = string.IsNullOrEmpty(obj.Slug) ? obj.Name.CHSToPinyin("-").ToLower() : obj.Slug.ToLower();
            
            //validate name existence
            if (!ValidateName(obj.Name, obj.Id.ToString()))
            {
                msg.Error = true;
                msg.Body = string.Format("Name [{0}] exists,please choose another one!",obj.Name);
                return msg;
            };
            
            using (var db = new GarfielderEntities())
            {
                if (obj.IsNew)//add new
                {
                    obj.Id = Guid.NewGuid();
                    dbm = new Group();
                    dbm.Id = obj.Id;
                    dbm.CreatedAt = DateTime.Now;
                    //TODO
                    dbm.CreatedBy = "Sys";
                    db.AddToGroups(dbm);
                }
                else
                { //update
                    dbm = db.Groups.SingleOrDefault(x => x.Id.Equals(obj.Id));
                    if (dbm == null)
                    { //has been deleted!
                        msg.Error = true;
                        msg.Body = string.Format("Obj {0} has been deleted!", obj.Id);
                        return msg;
                    }
                }//if

                dbm.Name = obj.Name;
                dbm.Slug = Group.CheckSlug(db, obj.Slug);
                dbm.Description = obj.Description;

                dbm.ParentID = obj.ParentID.Equals(Guid.Empty) ? default(Guid?) : obj.ParentID;
                dbm.Level = obj.Level;


                db.CommandTimeout = 0;
                db.SaveChanges();

                //clear cache
                ClearCache();

                if (dbm.Parent != null)
                {
                    obj.ParentName = dbm.Parent.Name;
                }

            };//using
            return obj;
        }

        #region private properties
        private static object _SyncRoot = new object();
        private static List<Group> _Groups;

        #endregion
    }
    
}
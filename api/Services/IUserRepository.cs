using Microsoft.AspNetCore.Http;
using pentoTrack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pentoTrack.Services
{
	public interface IUserRepository
	{
		User GetCurrentUser(HttpRequest request);
	}

	public class SingleUserRepository : IUserRepository
	{
		public User GetCurrentUser(HttpRequest request)
		{
			return new User
			{
				Id = "i-am-alone",
				Name = "Hari"
			};
		}
	}
}

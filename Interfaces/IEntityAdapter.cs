using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces
{
    public interface IEntityAdapter<T> where T : struct, IEntity
    {
        IEnumerable<T> ReadAll();
        T? Read(Guid id);
        void Save(T newData);
        void Delete(Guid id);
    }
}

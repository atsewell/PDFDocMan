using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PDFDocMan.Api.Db
{
    public class DocRepo
    {
        private readonly DocDbContext _context;

        public DocRepo(DocDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all the documents in the chosen order. Documents that have not been ordered will be sorted on the Create Date & Time.
        /// </summary>
        /// <returns>Zero or more documents from the database.</returns>
        public async Task<IEnumerable<Doc>> GetAll()
        {
            return await _context.Docs
                            .OrderBy(d => d.SortVal)
                            .ThenBy(d => d.CreateDt)
                            .ToListAsync();
        }

        /// <summary>
        /// Gets a document from the database from its id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The Doc entity</returns>
        /// <exception cref="NotFoundException">Thrown if the document is not found.</exception>
        public async Task<Doc> GetById(int id) => await _context.Docs.FirstOrDefaultAsync(d => d.Id == id) ?? throw new NotFoundException($"Document with id {id} not found");

        /// <summary>
        /// Gets documents from a list of ids.
        /// </summary>
        /// <param name="ids"></param>
        /// <returns>1 or more documents</returns>
        /// <exception cref="NotFoundException">Thrown if one or more documents are not found</exception>
        public async Task<IEnumerable<Doc>> GetByIds(int[] ids)
        {
            var docs = await _context.Docs.Where(d => ids.Contains(d.Id)).ToListAsync();

            if (docs.Count() != ids.Length)
            {
                var idsNotFound = ids.Where(i => !docs.Exists(d => d.Id == i));
                throw new NotFoundException($"{idsNotFound.Count()} ids not found; " + string.Join(',', idsNotFound));
            }

            return docs;
        }

        /// <summary>
        /// Creates a new document
        /// </summary>
        /// <param name="doc">The new Doc entity</param>
        /// <returns>The completed Doc entity</returns>
        public async Task<Doc> Create(Doc doc)
        {
            doc.CreateDt = DateTime.UtcNow;
            _context.Docs.Add(doc);
            await _context.SaveChangesAsync();
            return doc;
        }

        /// <summary>
        /// Deletes a document from the database
        /// </summary>
        /// <param name="id">The DocId</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">Thrown if the given document is not found</exception>
        public async Task Delete(int id)
        {
            var doc = await GetById(id);
            _context.Docs.Remove(doc);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Takes the list of document ids in the order that the user wishes for them to be sorted.
        /// Not sure about this, but the idea is that the user is able to sort them in the client app and then the ids are sent in the new sort order
        /// which avoids worrying about repositioning individual elements, but will only work well for short lists.
        /// </summary>
        /// <param name="ids">A list of all the document ids, in the new order.</param>
        /// <returns>The list of documents in the chosen order.</returns>
        /// <exception cref="ValidationException">Thrown if the incorrect number of ids are provided.</exception>
        /// <exception cref="NotFoundException">Thrown if one or more of the provided ids are not found in the db.</exception>
        public async Task<IEnumerable<Doc>> ReOrder(int[] ids)
        {
            var docs = await GetAll();
            if (docs.Count() != ids.Length)
            {
                throw new ValidationException($"Incorrect number of ids provided to reorder; expected {docs.Count()}, received {ids.Length}");
            }

            var missingIds = new List<int>();

            foreach(var idAndIdx in ids.Select((id, index) => new { id, index }))
            {
                var doc = docs.FirstOrDefault(d => d.Id == idAndIdx.id);
                if (doc == null)
                {
                    missingIds.Add(idAndIdx.id);
                }
                else
                {
                    doc.SortVal = idAndIdx.index + 1;
                }
            }

            if (missingIds.Any())
            {
                throw new NotFoundException("One or more document ids not found: " + string.Join(',', missingIds));
            }

            _context.Docs.UpdateRange(docs);
            await _context.SaveChangesAsync();

            docs = await GetAll();
            return docs;
        }
    }
}

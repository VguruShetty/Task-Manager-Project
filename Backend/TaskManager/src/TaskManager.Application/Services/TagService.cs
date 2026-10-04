using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Tags;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Mappings;

namespace TaskManager.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<TagDto>> GetAllAsync(CancellationToken ct = default)
        {
            var tags = await _tagRepository.GetAllAsync(ct);
            return tags.Select(t => t.ToDto());
        }

        public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var tag = await _tagRepository.GetByIdAsync(id, ct);
            return tag?.ToDto();
        }

        public async Task<TagDto> CreateAsync(CreateTagDto dto, CancellationToken ct = default)
        {
            var existing = await _tagRepository.GetByNameAsync(dto.Name.Trim(), ct);
            if (existing is not null)
            {
                throw new InvalidOperationException($"Tag with name '{dto.Name}' already exists.");
            }

            var tag = dto.ToEntity();
            var created = await _tagRepository.AddAsync(tag, ct);
            return created.ToDto();
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct = default)
        {
            var tag = await _tagRepository.GetByIdAsync(id, ct);
            if (tag is null) return false;

            var duplicate = await _tagRepository.GetByNameAsync(dto.Name.Trim(), ct);
            if (duplicate is not null && duplicate.Id != id)
            {
                throw new InvalidOperationException($"Tag with name '{dto.Name}' already exists.");
            }

            tag.Name = dto.Name.Trim();
            tag.ColorHex = dto.ColorHex.Trim();

            await _tagRepository.UpdateAsync(tag, ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _tagRepository.DeleteAsync(id, ct);
        }
    }
}

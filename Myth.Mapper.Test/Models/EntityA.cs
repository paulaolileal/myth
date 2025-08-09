using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Mapper.Test.Service;
using Myth.Repositories.Results;

namespace Myth.Mapper.Test.Models {

	internal enum TestEnum {
		None,
		One,
		Two
	}

	internal class EntityA : IMapTo<EntityA, Dto>, IMapTo<Paginated<EntityA>, Paginated<Dto>> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }
		public IEnumerable<EntityItem> ItemsProp { get; set; } = [ ];

		public void MapTo( MappingBuilder<EntityA, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.ForMember(
					dest => dest.ItemsProp,
					( src, sp ) => src.ItemsProp.MapTo<DtoItem>( ) );
		}

		public void MapTo( MappingBuilder<Paginated<EntityA>, Paginated<Dto>> builder ) {
			builder.ForMember(
				dest => dest.Items,
				src => src.Items.MapTo<Dto>( ) );
		}
	}

	internal class EntityB : IMapTo<EntityB, Dto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }
		public IEnumerable<EntityItem> ItemsProp { get; set; } = [ ];
		public IEnumerable<EntityItem> ItemsField = [ ];

		public void MapTo( MappingBuilder<EntityB, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.ForMember(
					dest => dest.ItemsField,
					( src, sp ) => src.ItemsField.MapTo<DtoItem>( ) )
				.ForMember(
					dest => dest.ItemsProp,
					( src, sp ) => src.ItemsProp.MapTo<DtoItem>( ) );
		}
	}

	internal class EntityC : IMapTo<EntityC, Dto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }
		public TestEnum Test { get; set; }
		public IEnumerable<EntityItem> ItemsProp { get; set; } = [ ];
		public IEnumerable<EntityItem> ItemsField = [ ];

		public void MapTo( MappingBuilder<EntityC, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.ForMember(
					dest => dest.TestId,
					( src, sp ) => ( int )src.Test )
				.ForMember(
					dest => dest.TestDescription,
					( src, sp ) => src.Test.ToString( ) )
				.ForMember(
					dest => dest.ItemsField,
					( src, sp ) => src.ItemsField.MapTo<DtoItem>( ) )
				.ForMember(
					dest => dest.ItemsProp,
					( src, sp ) => src.ItemsProp.MapTo<DtoItem>( ) );
		}
	}

	internal class EntityD : IMapTo<EntityD, Dto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }
		public TestEnum Test { get; set; }
		public IEnumerable<EntityItem> ItemsProp { get; set; } = [ ];
		public IEnumerable<EntityItem> ItemsField = [ ];

		public void MapTo( MappingBuilder<EntityD, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.ForMember(
					dest => dest.TestId,
					( src, sp ) => ( int )src.Test )
				.ForMember(
					dest => dest.TestDescription,
					( src, sp ) => src.Test.ToString( ) )
				.ForMember(
					dest => dest.ItemsField,
					( src, sp ) => src.ItemsField.MapTo<DtoItem>( ) )
				.ForMember(
					dest => dest.ItemsProp,
					( src, sp ) => src.ItemsProp.MapTo<DtoItem>( ) )
				.Ignore( dest => dest.Description );
		}
	}

	internal class EntityE : IMapTo<EntityE, Dto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public bool Enabled { get; set; }
		public string Description { get; set; }
		public TestEnum Test { get; set; }
		public IEnumerable<EntityItem> ItemsProp { get; set; } = [ ];
		public IEnumerable<EntityItem> ItemsField = [ ];

		public void MapTo( MappingBuilder<EntityE, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.Enabled,
					( src, sp ) => !src.Enabled )
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.ForMember(
					dest => dest.TestId,
					( src, sp ) => ( int )src.Test )
				.ForMember(
					dest => dest.TestDescription,
					( src, sp ) => src.Test.ToString( ) )
				.ForMember(
					dest => dest.ItemsField,
					( src, sp ) => src.ItemsField.MapTo<DtoItem>( ) )
				.ForMember(
					dest => dest.ItemsProp,
					( src, sp ) => src.ItemsProp.MapTo<DtoItem>( ) )
				.ForMember(
					dest => dest.Description,
					( src, sp ) => sp
						.GetRequiredService<IDescriptionResolver>( )
						.Resove( src.Description ) );
		}
	}

	internal class EntityF : IMapTo<EntityF, Dto> {
		public int EntityId { get; set; }
		public string Name { get; set; }
		public EntityItem Item { get; set; }

		public void MapTo( MappingBuilder<EntityF, Dto> builder ) {
			builder
				.ForMember(
					dest => dest.DtoId,
					( src, sp ) => src.EntityId )
				.ForMember(
					dest => dest.Item,
					( src, sp ) => src.Item.MapTo<DtoItem>( ) );
		}
	}

	internal class EntityItem : IMapTo<EntityItem, DtoItem> {
		public int ItemId { get; set; }
		public string Name { get; set; }

		public void MapTo( MappingBuilder<EntityItem, DtoItem> builder ) {
		}
	}
}
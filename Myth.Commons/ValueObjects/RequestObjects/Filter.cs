using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.RequestObjects {

    public class Filter<TViewModel> {
        public List<string> Filters { get; private set; }

        public Filter( ) {
            Filters = new List<string>( );
        }

        public Filter( string conditions ) {
            Filters = conditions.Split( '&', '?', StringSplitOptions.RemoveEmptyEntries ).ToList( ); ;
        }

        private string ConvertOperator( ExpressionType @operator ) {
            return ( @operator ) switch
            {
                ExpressionType.Equal => "eq",
                ExpressionType.NotEqual => "ne",
                ExpressionType.GreaterThan => "gt",
                ExpressionType.GreaterThanOrEqual => "ge",
                ExpressionType.LessThan => "lt",
                ExpressionType.LessThanOrEqual => "le",
                ExpressionType.And => "and",
                ExpressionType.Or => "or",
                ExpressionType.Not => "not",
                ExpressionType.Add => "add",
                ExpressionType.Subtract => "sub",
                ExpressionType.Multiply => "mul",
                ExpressionType.Divide => "div",
                ExpressionType.Modulo => "mod",
                _ => throw new Exception( "Operator not exists!" ),
            };
        }

        public void Add<TMember>( Expression<Func<TViewModel, TMember>> destinationMember ) {
            var binaryExpression = ( BinaryExpression ) destinationMember.Body;
            var memberLeftExpression = ( MemberExpression ) binaryExpression.Left;
            var property = memberLeftExpression.Member.Name;

            var memberRightExpression = ( ConstantExpression ) binaryExpression.Right;
            var value = memberRightExpression.Value;

            var @operator = ConvertOperator( binaryExpression.NodeType );

            Filters.Add( $"Filter={property} {@operator} {value}" );
        }

        public void Add( string condition ) {
            Filters.Add( condition );
        }

        public string Build( ) =>
            string.Join( "&", Filters.ToArray( ) );
    }
}
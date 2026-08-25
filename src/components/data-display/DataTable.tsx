import type { ReactNode } from 'react';
import type { SxProps, Theme } from '@mui/material/styles';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TablePagination from '@mui/material/TablePagination';
import TableRow from '@mui/material/TableRow';
import TableSortLabel from '@mui/material/TableSortLabel';
import Paper from '@mui/material/Paper';
import Skeleton from '@mui/material/Skeleton';
import type { SortDirection } from '@/types/pagination';
import { EmptyState } from '@/components/feedback/EmptyState';

export interface DataTableColumn<T> {
  id: string;
  label: string;
  sortable?: boolean;
  align?: 'left' | 'right' | 'center';
  render: (row: T) => ReactNode;
}

export interface DataTableProps<T> {
  caption: string;
  columns: DataTableColumn<T>[];
  rows: T[];
  getRowId: (row: T) => string;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  onPageChange: (pageNumber: number) => void;
  onPageSizeChange: (pageSize: number) => void;
  sortBy?: string;
  sortDirection?: SortDirection;
  onSortChange?: (sortBy: string) => void;
  onRowClick?: (row: T) => void;
  /** Optional per-row highlighting — e.g. tinting Critical-severity rows. A plain style-object bag (not the full responsive SxProps union), merged into the row's sx. */
  getRowSx?: (row: T) => Record<string, string | number> | undefined;
  isLoading?: boolean;
  emptyTitle?: string;
  emptyDescription?: string;
}

export function DataTable<T>({
  caption,
  columns,
  rows,
  getRowId,
  totalCount,
  pageNumber,
  pageSize,
  onPageChange,
  onPageSizeChange,
  sortBy,
  sortDirection = 'asc',
  onSortChange,
  onRowClick,
  getRowSx,
  isLoading = false,
  emptyTitle = 'No results',
  emptyDescription = 'Try adjusting your filters or search terms.',
}: DataTableProps<T>) {
  if (!isLoading && rows.length === 0) {
    return <EmptyState title={emptyTitle} description={emptyDescription} />;
  }

  return (
    <Paper variant="outlined">
      <TableContainer sx={{ overflowX: 'auto' }}>
        <Table aria-label={caption} sx={{ minWidth: 640 }}>
          <TableHead>
            <TableRow>
              {columns.map((column) => (
                <TableCell key={column.id} align={column.align ?? 'left'}>
                  {column.sortable && onSortChange ? (
                    <TableSortLabel
                      active={sortBy === column.id}
                      direction={sortBy === column.id ? sortDirection : 'asc'}
                      onClick={() => onSortChange(column.id)}
                    >
                      {column.label}
                    </TableSortLabel>
                  ) : (
                    column.label
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {isLoading
              ? Array.from({ length: Math.min(pageSize, 5) }).map((_, rowIndex) => (
                  <TableRow key={rowIndex}>
                    {columns.map((column) => (
                      <TableCell key={column.id}>
                        <Skeleton variant="text" />
                      </TableCell>
                    ))}
                  </TableRow>
                ))
              : rows.map((row) => (
                  <TableRow
                    key={getRowId(row)}
                    hover={Boolean(onRowClick)}
                    onClick={onRowClick ? () => onRowClick(row) : undefined}
                    tabIndex={onRowClick ? 0 : undefined}
                    onKeyDown={
                      onRowClick
                        ? (event) => {
                            if (event.key === 'Enter' || event.key === ' ') {
                              event.preventDefault();
                              onRowClick(row);
                            }
                          }
                        : undefined
                    }
                    sx={
                      {
                        ...(onRowClick ? { cursor: 'pointer' } : {}),
                        ...(getRowSx?.(row) ?? {}),
                      } as SxProps<Theme>
                    }
                  >
                    {columns.map((column) => (
                      <TableCell key={column.id} align={column.align ?? 'left'}>
                        {column.render(row)}
                      </TableCell>
                    ))}
                  </TableRow>
                ))}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        component="div"
        count={totalCount}
        page={pageNumber - 1}
        rowsPerPage={pageSize}
        onPageChange={(_event, newPage) => onPageChange(newPage + 1)}
        onRowsPerPageChange={(event) => onPageSizeChange(Number(event.target.value))}
        rowsPerPageOptions={[10, 20, 50]}
      />
    </Paper>
  );
}
